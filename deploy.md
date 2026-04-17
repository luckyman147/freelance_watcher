# Deployment Guide for Render.com

This guide outlines the steps to deploy the .NET 8 Worker Service to Render.com as a Background Worker using Docker.

## Prerequisites

1. A **Render.com** account.
2. The code pushed to a GitHub/GitLab registry.

## Infrastructure Resources Required
On Render:
- **PostgreSQL Database** (Render provides a managed PostgreSQL service with a free tier).
- **Background Worker** (Render offers a free or very cheap background worker tier).

## Step-by-Step Deployment (Render Dashboard)

1. **Create the Database:**
   - Go to the Render Dashboard -> New -> **PostgreSQL**.
   - Fill in the details (Name: `azurewatcher-db`).
   - Click **Create Database**.
   - Copy the "Internal Database URL".

2. **Deploy the Background Worker:**
   - Go to the Render Dashboard -> New -> **Background Worker**.
   - Connect your GitHub repository containing the project.
   - Configure the service:
     - **Name**: `job-scraper-worker`
     - **Environment**: `Docker` (Render will automatically detect the `Dockerfile` at the root).

3. **Configure Environment Variables:**
   Under the **Environment** section, add the following variables:
   - `ConnectionStrings__DefaultConnection`: `<The Internal Database URL from Step 1>`
   - `TargetWebsiteUrl`: `https://tunisiefreelance.tn/fr/search`
   - `SmtpHost`: `smtp.gmail.com` (or your provider's SMTP host)
   - `SmtpPort`: `587` (typically 587 for TLS, or 465 for SSL)
   - `SmtpUsername`: `<Your SMTP Username/Email>`
   - `SmtpPassword`: `<Your SMTP Password or App Password>`
   - `EmailFromAddress`: `noreply@yourdomain.com` (Ensure this is allowed by your SMTP provider)
   - `EmailToAddress`: `youremail@yourdomain.com`
   
   *(Note: The `__` in the ConnectionString var represents the nested JSON structure `ConnectionStrings:DefaultConnection` in appsettings.json).*

4. **Deploy**:
   - Click **Create Background Worker**. Render will build the Docker container and start running your loop! 

## Monitoring

- Once running, the worker will execute every 10 minutes.
- The `Dockerfile` handles the entire build process.
- On startup, the generic host will automatically apply Entity Framework Core migrations to your PostgreSQL database.
- You can monitor execution logs from the Logs tab of your Background Worker in Render.
