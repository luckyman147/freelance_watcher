# Deployment Guide for Render.com

This guide outlines the steps to deploy the .NET 8 App to Render.com as a **Web Service** using Docker.

## Prerequisites

1. A **Render.com** account.
2. The code pushed to a GitHub/GitLab registry.

## Deployment Options

### Option A: Manual Deployment (Easiest for quick setup)
Follow the "Step-by-Step Deployment" section below.

### Option B: Render Blueprints (Recommended for Automation)
1. In the Render Dashboard, go to **Blueprints**.
2. Connect this repository.
3. Render will use the `render.yaml` file to automatically provision the Database and Web Service with all configurations!

## Step-by-Step Deployment (Render Dashboard)

1. **Create the Database:**
   - Go to the Render Dashboard -> New -> **PostgreSQL**.
   - Fill in the details (Name: `azurewatcher-db`).
   - Click **Create Database**.
   - Copy the "Internal Database URL".

2. **Deploy the Web Service:**
   - Go to the Render Dashboard -> New -> **Web Service**.
   - Connect your GitHub repository containing the project.
   - Configure the service:
     - **Name**: `job-scraper-service`
     - **Environment**: `Docker`
     - **Instance Type**: `Starter` (or Free if available)

3. **Configure Environment Variables:**
   Under the **Environment** section, add the following variables:
   - `DATABASE_URL`: `<The Internal Database URL from Step 1>` (Optional: if this is set, it will be used automatically).
   - `ConnectionStrings__DefaultConnection`: `<The Internal Database URL>` (Fallback if DATABASE_URL is not set).
   - `TargetWebsiteUrl`: `https://tunisiefreelance.tn/fr/search`
   - `SmtpHost`: `smtp.gmail.com` (or your provider's SMTP host)
   - `SmtpPort`: `587` (typically 587 for TLS, or 465 for SSL)
   - `SmtpUsername`: `<Your SMTP Username/Email>`
   - `SmtpPassword`: `<Your SMTP Password or App Password>`
   - `EmailFromAddress`: `noreply@yourdomain.com` (Ensure this is allowed by your SMTP provider)
   - `EmailToAddress`: `youremail@yourdomain.com`
   
   *(Note: The `__` in the ConnectionString var represents the nested JSON structure `ConnectionStrings:DefaultConnection` in appsettings.json).*

4. **Deploy**:
   - Click **Create Web Service**. Render will build the Docker container, run migrations, and start both the HTTP server and your background monitoring logic! 

## Monitoring

- Once running, the worker will execute every 1 hour (as configured in `Worker.cs`).
- The `Dockerfile` handles the entire build process and exposes port 10000.
- On startup, the app automatically applies Entity Framework Core migrations to your PostgreSQL database.
- You can verify the service is alive by visiting the public URL of your Web Service.
- Monitor execution logs from the Logs tab in Render.
