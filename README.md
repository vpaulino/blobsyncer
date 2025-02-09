# BlobSyncer.Azure.BlobStorage

**BlobSyncer.Azure.BlobStorage** is a tool designed to **download files from Azure Blob Storage and store them in the filesystem**. It is structured to support additional **syncing features** in the future, allowing files to be synced to another Blob Storage using **DestinationHandlers**.

---

## 🚀 Features
- **Download files from Azure Blob Storage** and save them to the local filesystem.
- **Extendable architecture** for adding support to sync files to different destinations.
- **Uses Channels for parallel processing**, improving performance.

---

## 📂 Project Structure

BlobSyncer.Azure.BlobStorage 
│── Dependencies 
│── Channels 
│ ├── Readers 
│ │ ├── FileSystem 
│ │ │ ├── FileSystemDestinationHandler.cs # Handles writing to the filesystem 
│ │ ├── IChannelReader.cs # Interface for reading from a channel 
│ │ ├── IDestinationHandler.cs # Interface for handling different sync destinations 
│ │ ├── PageBlobItemChannelReader.cs # Reads Page Blob items from a channel 
│ ├── Writers 
│ │ ├── IChannelWriter.cs # Interface for writing to a channel 
│ │ ├── PageBlobItemChannelWriter.cs # Writes Page Blob items to a channel 
│── AzureBlobDownloader.cs # Core component handling Azure Blob downloads 
│── DownloadSettings.cs # Configuration settings for downloads


---

## 🔧 Installation

1. Clone this repository:
   ```sh
   git clone https://github.com/yourusername/BlobSyncer.Azure.BlobStorage.git
   cd BlobSyncer.Azure.BlobStorage

## 📌  Roadmap
  - [ ] Add support for syncing files to another Azure Blob Storage.
   - [ ] Add support to send notifications to a pubsub channel everytime one container is updated from a syncronization
  - [ ] Add logging and monitoring for better observability.


## 📜 License

This project is licensed under the MIT License.
