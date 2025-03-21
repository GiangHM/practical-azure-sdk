# Practical Azure SDK

This repository contains various projects that serve as practice exercises for working with the .NET Azure SDK. The goal of these projects is to help you learn about different Azure services by using them in real-world scenarios and implementations.

## Description

This repo demonstrates how to interact with some Azure Service such as: Azure Blob Storage, Azure Table Storage, Azure Redis Cache using the Azure SDK for .NET.
It contains a wrapper around the `Azure SDKs` library to simplify its usage.

The project exposes two methods for injecting the BlobClient into the Dependency Injection (DI) container:
- **Managed Identity (Recommended)**: Leverages Azure’s Managed Identity to securely authenticate and interact with Blob Storage.
- **Connection String**: Uses a connection string to authenticate and access Blob Storage.

## Getting Started

To get started with any project in this repository, follow these steps:

### Prerequisites
- .NET 6.0 or later
- An Azure account and subscription
- Azure SDK for .NET libraries installed
- Azure Storage Account for testing Blob Storage features

### Installation

1. Clone this repository to your local machine:
   ```bash
   git clone https://github.com/GiangHM/practical-azure-sdk.git
   ```

2. Navigate to the specific project directory:
   ```bash
   cd AzureBlobStorage
   ```

3. Restore the project dependencies:
   ```bash
   dotnet restore
   ```

### Usage

Follow the instructions specific to the project you are working on. For example, in the **Azure Blob Storage** project, you can configure your chosen method (Managed Identity or Connection String) and test the interaction with your Azure Blob Storage instance.

