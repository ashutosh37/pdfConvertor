# PDF Converter Azure Function

This project provides an Azure Function that converts various file types (.msg, .png, .jpg, .jpeg, .docx) to PDF format. It's designed to be run as a containerized application.

## Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for building, if not using only Docker)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
*   [Postman](https://www.postman.com/downloads/) (or any other API client)
*   An Azure Storage Account connection string (or [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=docker-hub) installed and running for local emulation).

## Setup

1.  **Clone the repository:**
    ```bash
    git clone <your-repository-url>
    cd pdfConvertor
    ```
2.  **Get Azure Storage Connection String:** The Azure Functions host requires a connection string to operate.
    *   **Azure Portal:** Navigate to your Storage Account -> Security + networking -> Access keys -> Show -> Copy the Connection string.
    *   **Azurite (Local Emulator):** If Azurite is running, you can use the development connection string: `UseDevelopmentStorage=true`
3.  **(Optional) Create `.dockerignore` file:** To speed up Docker builds, create a `.dockerignore` file in the `pdfConvertor` directory with the following content:
    ```
    **/.dockerignore
    **/.env
    **/.git
    **/.gitignore
    **/.vs
    **/.vscode
    **/bin/
    **/obj/
    Dockerfile*
    docker-compose*
    *.user
    *.suo
    README.md
    ```

## Building the Docker Image

Navigate to the `pdfConvertor` directory in your terminal and run:

```bash
docker build -t pdfconvertor:1.0 .
```

## Running the Docker Container

Run the following command, replacing the placeholder connection string:

```bash
# Replace YOUR_AZURE_STORAGE_CONNECTION_STRING with your actual connection string or UseDevelopmentStorage=true for Azurite
docker run -d -p 8080:80 -e AzureWebJobsStorage="YOUR_AZURE_STORAGE_CONNECTION_STRING" --name pdfconverter-run pdfconvertor:1.0
```

*   `-d`: Runs the container in detached mode (background).
*   `-p 8080:80`: Maps port 8080 on your local machine to port 80 inside the container (where the function host listens).
*   `-e AzureWebJobsStorage=...`: Provides the necessary storage connection string.
*   `--name pdfconverter-run`: Assigns a name to the container for easy management.

## Testing with Postman

1.  Open Postman.
2.  Create a new request.
3.  Set the request type to `POST`.
4.  Set the URL to `http://localhost:8080/convert`.
5.  Go to the **Body** tab.
6.  Select the `form-data` option.
7.  In the `KEY` column, enter `file`.
8.  In the same row, hover over the `VALUE` column and click the dropdown that appears, changing `Text` to `File`.
9.  Click the `Select Files` button that appears and choose the file you want to convert (e.g., a `.msg`, `.png`, or `.docx` file).
10. Click **Send**.

**Expected Response:**
*   **Status:** `200 OK`
*   **Body:** The response body will contain the binary data of the converted PDF file. Postman might prompt you to save the response as a file (e.g., `response.pdf`).
*   **Headers:** Includes `Content-Type: application/pdf` and `Content-Disposition: attachment; filename=converted.pdf`.

## Supported Input Formats

*   `.msg` (Outlook Message File)
*   `.png`
*   `.jpg` / `.jpeg`
*   `.docx` (Requires LibreOffice installed in the container, which the Dockerfile handles)

## Stopping the Container

```bash
docker stop pdfconverter-run
docker rm pdfconverter-run # Optional: remove the stopped container
```