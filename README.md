# PDF Converter Azure Function
 
This project provides an Azure Function that converts various file types (.msg, .png, .jpg, .jpeg, .docx) to PDF format. It's designed to be run as a containerized application.

## Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for building, if not using only Docker)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
*   [Postman](https://www.postman.com/downloads/) (or any other API client)
*   An Azure Storage Account connection string (or [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=docker-hub) installed and running for local emulation).
*   [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) (for running locally without Docker)

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

## Running Locally with `func start` (Without Docker)

This method uses the Azure Functions Core Tools to run the function directly on your machine. Note that for `.docx` conversion, this requires having LibreOffice (specifically the `soffice` command) installed and available in your system's PATH.

1.  **Set Environment Variable:** The function requires the `AzureWebJobsStorage` connection string. Set this as an environment variable in your terminal session or configure it in `local.settings.json`.
    *   **Terminal (Example - Bash/Zsh):**
        ```bash
        export AzureWebJobsStorage="YOUR_AZURE_STORAGE_CONNECTION_STRING"
        ```
    *   **Terminal (Example - PowerShell):**
        ```powershell
        $env:AzureWebJobsStorage = "YOUR_AZURE_STORAGE_CONNECTION_STRING"
        ```
    *   **`local.settings.json` (Create this file in the `pdfConvertor` directory if it doesn't exist):**
        ```json
        {
          "IsEncrypted": false,
          "Values": {
            "AzureWebJobsStorage": "YOUR_AZURE_STORAGE_CONNECTION_STRING",
            "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
          }
        }
        ```
        *(Replace `YOUR_AZURE_STORAGE_CONNECTION_STRING` with your actual connection string or `UseDevelopmentStorage=true` for Azurite)*

2.  **Navigate to Project Directory:**
    ```bash
    cd pdfConvertor
    ```
3.  **Run the Function Host:**
    ```bash
    func start
    ```
4.  **Note the Endpoint URL:** The Core Tools will output the URL where the function is listening, typically `http://localhost:7071/api/convert`. Note the port number (e.g., 7071) and the `/api/` prefix, which is common when running locally with `func start`.

## Testing with Postman

1.  Open Postman.
2.  Create a new request.
3.  Set the request type to `POST`.
4.  Set the URL to `http://localhost:8080/convert`.
5.  Go to the **Body** tab.
6.  Select the `form-data` option and add Key as "file" and select file type for value instead of text and upload file.

### Testing with Postman (Docker Container)

1.  Set the URL to `http://localhost:8080/convert` (or the host port you mapped).
7.  In the `KEY` column, enter `file`.
8.  In the same row, hover over the `VALUE` column and click the dropdown that appears, changing `Text` to `File`.
9.  Click the `Select Files` button that appears and choose the file you want to convert (e.g., a `.msg`, `.png`, or `.docx` file).
10. Click **Send**.

**Expected Response:**
*   **Status:** `200 OK`
*   **Body:** The response body will contain the binary data of the converted PDF file. Postman might prompt you to save the response as a file (e.g., `response.pdf`).
*   **Headers:** Includes `Content-Type: application/pdf` and `Content-Disposition: attachment; filename=converted.pdf`.

### Testing with Postman (Local `func start`)

1.  Set the URL to the one provided by `func start`, typically `http://localhost:7071/api/convert`. Make sure to include the `/api/` prefix and the correct port.
2.  Go to the **Body** tab.
3.  Select the `form-data` option.
4.  In the `KEY` column, enter `file`.
5.  In the same row, hover over the `VALUE` column and click the dropdown that appears, changing `Text` to `File`.
6.  Click the `Select Files` button that appears and choose the file you want to convert (e.g., a `.msg`, `.png`, or `.docx` file).
7.  Click **Send**.

**Expected Response:**
*   Same as the Docker container response (Status 200 OK, PDF body, correct headers).


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