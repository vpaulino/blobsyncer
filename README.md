﻿# BlobSyncer.Azure.BlobStorage

**BlobSyncer.Azure.BlobStorage** is a tool designed to **download files from Azure Blob Storage and store them in the filesystem**. It is structured to support additional **syncing features** in the future, allowing files to be synced to another Blob Storage using **DestinationHandlers**.

---

## Build Status 

![Build Status](https://github.com/vpaulino/blobsyncer/actions/workflows/dotnet-build.yml/badge.svg)

## Nuget 

[![NuGet](https://img.shields.io/nuget/v/BlobSyncer.svg)](https://www.nuget.org/packages/BlobSyncer/)


## 🚀 Features
- **Download files from Azure Blob Storage** and save them to the local filesystem.
- **Extendable architecture** for adding support to sync files to different destinations.
- **Uses Channels for parallel processing**, improving performance.

---

## 📂 Project Structure

```
📦 BlobSyncer.Azure.BlobStorage
│── 📁 .github
│   ├── 📁 workflows
│   │   ├── 📄 dotnet-build.yml    # GitHub Actions pipeline for CI/CD
│── 📁 src                        # Library projects
│   ├── 📁 BlobSyncer.Azure.BlobStorage
│   │   ├── 📁 Channels
│   │   │   ├── 📁 Readers
│   │   │   │   ├── 📁 FileSystem
│   │   │   │   │   ├── 📄 FileSystemDestinationHandler.cs
│   │   │   │   ├── 📄 IChannelReader.cs
│   │   │   │   ├── 📄 IDestinationHandler.cs
│   │   │   │   ├── 📄 PageBlobItemChannelReader.cs
│   │   │   ├── 📁 Writers
│   │   │   │   ├── 📄 IChannelWriter.cs
│   │   │   │   ├── 📄 PageBlobItemChannelWriter.cs
│   │   ├── 📄 AzureBlobDownloader.cs
│   │   ├── 📄 DownloadSettings.cs
│── 📁 samples                    # Console applications
│   ├── 📁 SampleApp1              # Example console app using BlobSyncer
│   ├── 📁 SampleApp2
│── 📄 BlobSyncer.Azure.BlobStorage.sln  # Solution file
│── 📄 README.md
```



---

## 🔧 Installation

1. Clone this repository:
   ```sh
   git clone https://github.com/yourusername/BlobSyncer.Azure.BlobStorage.git
   cd BlobSyncer.Azure.BlobStorage

## 📌  Roadmap
  - [X] Add support for syncing files to another Azure Blob Storage.
   - [X] Add support for syncing files to Amazon S3.
	- [X] Add support for syncing files to Azure Search AI by generate embendings.
	- [ ] Add Support to process multiple destinations different strategies to orchestrate destinations
   - [ ] Add support to send notifications to a pubsub channel everytime one container is updated from a syncronization
  


## 📜 License

This project is licensed under the MIT License.
