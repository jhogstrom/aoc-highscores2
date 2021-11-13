from aws_cdk import (
    aws_dynamodb,
    aws_s3,
    aws_sqs,
    aws_iam,
    aws_apigateway,
    aws_lambda,
    aws_lambda_event_sources,
    core as cdk
)
from aws_cdk.aws_iam import PolicyStatement
import aws

# For consistency with other languages, `cdk` is the preferred import name for
# the CDK's core module.  The following line also imports it as `core` for use
# with examples from the CDK Developer's Guide, which are in the process of
# being updated to use `cdk`.  You may delete this import if you don't need it.
from aws_cdk import core


class AoCHSStack(cdk.Stack):

    def __init__(self, scope: cdk.Construct, construct_id: str, **kwargs) -> None:
        super().__init__(scope, construct_id, **kwargs)

        # Queue with refresh requests
        refreshQ = aws.Queue(self, "aoc_refreshQ")
        # Queue with regeneration requests
        regenerateQ = aws.Queue(self, "aoc_regenerateQ", visibility_timeout=cdk.Duration.seconds(300))  # Must be >= function timeout

        # Throttler receives regeneration messages and determines if they require regeneration
        throttler = aws.Function(self, "throttler", environment={
            "REGENERATOR_Q": regenerateQ.queue_url
        })
        throttler.add_event_source(aws_lambda_event_sources.SqsEventSource(refreshQ, batch_size=10))
        regenerateQ.grant_send_messages(throttler)

        #  Regenerator is written in C#. It eats messages from regeneratorQ.
        regenerator = aws_lambda.Function.from_function_attributes(
            self,
            "regenerator",
            function_arn="arn:aws:lambda:us-east-2:253686873989:function:aochc-refreshaoc",
            role=aws_iam.Role.from_role_arn(self, "regenerator_role", "arn:aws:iam::253686873989:role/lambda_exec_aochc-refreshaoc-2"),
            same_environment=True)
        regenerator.add_event_source(aws_lambda_event_sources.SqsEventSource(regenerateQ, batch_size=1))

        # S3 bucket for public pages + data-blobs
        # aws_s3.Bucket(blo
        website = aws.Bucket(
            self,
            "website",
            website_index_document="index.html",
            website_error_document="error.html",
            block_public_access=None,
            public_read_access=True,
            cors=[aws_s3.CorsRule(
                allowed_methods=[aws_s3.HttpMethods.GET],
                allowed_headers=["*"],
                allowed_origins=["*"])])
        website.grant_public_access()
        website.grant_read_write(regenerator)

        # S3 bucket with cached AOC-datablobs
        cache = aws.Bucket(self, "cache")
        cache.grant_read_write(regenerator)

        # DynamoDB table with listguid + timestamp for latest regeneration request
        timestamps = aws.Table(self, "timestamps")
        timestamps.grant_read_write_data(throttler)
        throttler.add_environment("TIMESTAMPDB", timestamps.table_name)

        # DynamoDB table with listguid + config for all boards
        boardconfig = aws.Table(
            self,
            "boardsconfig",
            sort_key=aws_dynamodb.Attribute(name="sk", type=aws_dynamodb.AttributeType.STRING))
        boardconfig.grant_read_write_data(regenerator)

        layers = aws.PipLayers(self, "layers", layers={"api": "backend/api_requirements.txt"})

        # API GW + handler for public API
        public_api_handler = aws.Function(self, "public_api_handler", layers=layers.layers)
        public_api = aws_apigateway.LambdaRestApi(self, "public_api", handler=public_api_handler)
        refreshQ.grant_send_messages(public_api_handler)
        public_api_handler.add_environment("REFRESHQ", refreshQ.queue_url)

        # API GW + handler for private API
        admin_api_handler = aws.Function(self, "admin_api_handler", layers=layers.layers)
        admin_api = aws_apigateway.LambdaRestApi(self, "admin_api", handler=admin_api_handler)
        admin_api_handler.add_environment("CONFIGDB", boardconfig.table_name)
        boardconfig.grant_read_write_data(admin_api_handler)

