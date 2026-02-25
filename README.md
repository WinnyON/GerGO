# Mini DBMS

A lightweight relational database management system built from scratch as a university team project.

## Overview

Mini DBMS is a fully functional relational database engine with a graphical user interface. The system consists of a **.NET backend** that handles all database logic and a **Python PyQt frontend** that communicates with the backend over a custom TCP socket protocol.

## Features

- **Core Database Operations:** CRUD, selection, projection, and grouping
- **Indexing:** Efficient indexed storage enabling insertion of 100k+ rows in minutes
- **Join Algorithms:** Automatic join algorithm selection (e.g., hash join) based on query characteristics
- **Transactions:** ACID-compliant transaction support
- **Query Parsing:** Custom query parser for structured data retrieval
- **Custom Socket Protocol:** Batched data transfer between the Python PyQt UI and the .NET backend for optimized performance

## Architecture

```
┌─────────────────────┐        TCP Socket        ┌─────────────────────┐
│   Python PyQt UI    │ ◄──────────────────────► │    .NET Backend     │
│     (client/)       │    Custom Protocol        │     (server/)       │
└─────────────────────┘                           └─────────────────────┘
```

## Tech Stack

- **Backend:** C#, .NET
- **Frontend:** Python, PyQt
- **Communication:** TCP Socket Programming
- **Version Control:** Git

## Getting Started

### Prerequisites

- .NET SDK
- Python 3.x with PyQt5

### Running the Application

1. Start the backend server from the `server/` directory.
2. Launch the PyQt client from the `client/` directory.
3. Connect the client to the server and start managing your databases.
