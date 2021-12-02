@echo off
if not exist deployenv.bat goto no_env
call deployenv.bat
rem By default the default profile or AWS_PROFILE fill be used. You may want to set it in deployenv.bat

echo Building dist...
call npm run build

echo Deploying to S3...
aws s3 cp dist s3://aochsstack-website --recursive

echo Invalidating CloudFront...
aws cloudfront create-invalidation --distribution-id %CLOUDFRONT_ID% --paths "/index.html"

goto end


:no_env
echo Create deployenv.bat and set appropriate environment variables
:end