docker run --platform=linux/amd64 -p 8080:8080 -e AzureWebJobsStorage="" -t pdfconvertor:1.0 

docker run -d --platform=linux/amd64 -p 8080:80  --name pdfconverter-run pdfconvertor:1.0
