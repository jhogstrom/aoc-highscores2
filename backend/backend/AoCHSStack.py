from aws_cdk import (
    aws_certificatemanager,
    aws_dynamodb,
    aws_s3,
    aws_cloudfront,
    aws_sqs,
    aws_iam,
    aws_apigateway,
    aws_lambda,
    aws_lambda_event_sources,
    core as cdk
)
from aws_cdk.aws_iam import Effect, PolicyStatement
import aws
import os

# For consistency with other languages, `cdk` is the preferred import name for
# the CDK's core module.  The following line also imports it as `core` for use
# with examples from the CDK Developer's Guide, which are in the process of
# being updated to use `cdk`.  You may delete this import if you don't need it.
from aws_cdk import core
from dotenv import dotenv_values

curdir = os.path.dirname(os.path.abspath(__file__))
envfile = f'{curdir}/.env'

config = {
    **dotenv_values(envfile),  # load shared development variables
    # **os.environ,  # override loaded values with environment variables
}

class AoCHSStack(cdk.Stack):

    def __init__(self, scope: cdk.Construct, construct_id: str, **kwargs) -> None:
        super().__init__(scope, construct_id, **kwargs)
        SES_REGION = cdk.Stack.of(self).region

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

        routing_rules = []
        error_responses = []
        for error_code in ["403", "404"]:
            routing_rules.append(
                aws_s3.RoutingRule(
                    condition=aws_s3.RoutingRuleCondition(
                        http_error_code_returned_equals=error_code,
                    ),
                    replace_key=aws_s3.ReplaceKey.prefix_with("#!")
                )
            )
            error_responses.append(aws_cloudfront.CfnDistribution.CustomErrorResponseProperty(
                        error_code=int(error_code),
                        response_page_path="/index.html",
                        response_code=200))

        # S3 bucket for public pages + data-blobs
        website = aws.Bucket(
            self,
            "website",
            website_index_document="index.html",
            website_error_document="index.html",
            block_public_access=None,
            public_read_access=True,
            website_routing_rules=routing_rules,
            cors=[aws_s3.CorsRule(
                allowed_methods=[aws_s3.HttpMethods.GET],
                allowed_headers=["*"],
                allowed_origins=["*"])])
        website.grant_public_access()
        website.grant_read_write(regenerator)
        core.CfnOutput(self, "S3WebUrl", value=website.bucket_website_url, )

        aoc_certificate = aws_certificatemanager.Certificate.from_certificate_arn(
            self,
            "aoc_certificate",
            "arn:aws:acm:us-east-1:253686873989:certificate/ea53b805-91a4-4d7e-8bb4-b6848f34b760"
        )

        distribution = aws_cloudfront.CloudFrontWebDistribution(
            self,
            "AocCdn",
            comment="AoC Highscore CDN",
            origin_configs=[aws_cloudfront.SourceConfiguration(
                s3_origin_source=aws_cloudfront.S3OriginConfig(
                    s3_bucket_source=website
                ),
                behaviors=[aws_cloudfront.Behavior(is_default_behavior=True)],
            )
            ],
            error_configurations=error_responses,
            viewer_certificate=aws_cloudfront.ViewerCertificate.from_acm_certificate(
                aoc_certificate,
                aliases=["aoc.lillfiluren.se"])
        )
        core.CfnOutput(self, "CDNUrl", value=distribution.distribution_domain_name)


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
            sort_key=aws_dynamodb.Attribute(name="sk", type=aws_dynamodb.AttributeType.STRING),
            billing_mode=aws_dynamodb.BillingMode.PAY_PER_REQUEST
        )
        boardconfig.grant_read_write_data(regenerator)
        # DynamoDB table with listguid + config for all boards
        playerdata = aws.Table(
            self,
            "playerdata",
            sort_key=aws_dynamodb.Attribute(
                name="sk",
                type=aws_dynamodb.AttributeType.STRING),
            billing_mode=aws_dynamodb.BillingMode.PAY_PER_REQUEST
        )
        playerdata.grant_read_write_data(regenerator)

        globalscores = aws.Table(
            self,
            "globalscores",
            partition_key=aws_dynamodb.Attribute(name="year", type=aws_dynamodb.AttributeType.NUMBER),
            sort_key=aws_dynamodb.Attribute(name="day", type=aws_dynamodb.AttributeType.NUMBER),
            billing_mode=aws_dynamodb.BillingMode.PAY_PER_REQUEST
        )
        globalscores.grant_read_write_data(regenerator)

        layers = aws.PipLayers(self, "layers", layers={"api": "backend/api_requirements.txt"})

        # API GW + handler for public API
        public_api_handler = aws.Function(
            self,
            "public_api_handler",
            layers=layers.layers,
            handler="public_api_handler.handler")
        public_api = aws_apigateway.LambdaRestApi(
            self,
            "public_api",
            description="AOC-Public API",
            handler=public_api_handler)
        refreshQ.grant_send_messages(public_api_handler)
        public_api_handler.add_environment("REFRESHQ", refreshQ.queue_url)
        public_api_handler.add_environment("SES_EMAIL_FROM", config['SES_EMAIL_FROM'])
        boardconfig.grant_read_write_data(public_api_handler)
        public_api_handler.add_environment("CONFIGDB", boardconfig.table_name)
        public_api_handler.add_to_role_policy(PolicyStatement(
            actions=[
                'ses:SendEmail',
                'ses:SendRawEmail',
                'ses:SendTemplatedEmail'
            ],
            effect=Effect.ALLOW,
            resources=[f"arn:aws:ses:{SES_REGION}:{cdk.Stack.of(self).account}:identity/{config['SES_EMAIL_FROM']}"]

        ))

        # API GW + handler for private API
        admin_api_handler = aws.Function(
            self,
            "admin_api_handler",
            layers=layers.layers,
            handler="admin_api_handler.handler")
        admin_api = aws_apigateway.LambdaRestApi(
            self,
            "admin_api",
            description="AOC-Admin API",
            handler=admin_api_handler)
        admin_api_handler.add_environment("CONFIGDB", boardconfig.table_name)
        boardconfig.grant_read_write_data(admin_api_handler)
