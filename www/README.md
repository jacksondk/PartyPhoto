# PartyPhoto - Azure Static Web App

This project is set up as an Azure Static Web App with an integrated API for image uploads.

## Project Structure

- `/www` - Root folder for the Azure Static Web App
  - `/api` - Contains the Azure Functions API
  - `index.html`, `styles.css`, `app.js` - Frontend web application
  - `staticwebapp.config.json` - Azure Static Web App configuration

## Features

- Image upload with file validation
- Progress tracking during upload
- Responsive design for all device sizes
- Backend validation of file types and size limits

## Development

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Visual Studio Code](https://code.visualstudio.com/) with the following extensions:
  - [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
  - [Azure Static Web Apps](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurestaticwebapps)

### Running Locally

The project includes VS Code tasks to help with local development:

1. Open the project in VS Code
2. Press F5 or use the Run and Debug panel to start debugging
3. Alternatively, use the "start local development" task which will:
   - Start the API on port 7071
   - Serve the frontend on port 8080

You can also run these components manually:

1. Start the API:
   ```pwsh
   cd www/api
   func start
   ```

2. Serve the frontend using any static file server:
   ```pwsh
   cd www
   python -m http.server 8080
   # or
   npx serve
   ```

### Testing the API

You can test the API endpoints directly:

- Health check: GET http://localhost:7071/api/Health
- Upload: POST http://localhost:7071/api/Upload (multipart/form-data with a file field)

## Deployment

### Azure Static Web Apps

1. Create an Azure Static Web App resource in the Azure Portal
2. Connect it to your GitHub repository
3. Configure the build settings:
   - App location: `/www`
   - API location: `/www/api`
   - Output location: `/`

The included GitHub workflow file (`.github/workflows/azure-static-web-apps.yml`) will handle the deployment process.

### Manual Deployment

You can also deploy the components separately:

1. Deploy the API as an Azure Function App:
   ```pwsh
   cd www/api
   func azure functionapp publish YOUR_FUNCTION_APP_NAME
   ```

2. Deploy the frontend to any static hosting service (Azure Blob Storage, Netlify, Vercel, etc.)

## Future Enhancements

- User authentication
- Image gallery view
- Image editing capabilities
- Storage integration with Azure Blob Storage
