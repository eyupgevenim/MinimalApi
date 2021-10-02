.Net Minimal Api
================

Buiding docker image:<br />
Run commands in solution directory
```docker
#docker build -f "PROJECT_PATH\Dockerfile" -t IMAGE_NAME:dev "SOLUTION_PATH"
_> docker build -f ".\MinimalApi\Dockerfile" -t minimal-api:dev "."

#run
_> docker run -p 5000:80 minimal-api:dev
```


