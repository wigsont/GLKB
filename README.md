GLKB
C# Source Code for Generating Geological Complex Labels Using a Knowledge Base
This project demonstrates how to automatically generate geological complex labels using a domain-specific knowledge base. Each label is generated in approximately 3 milliseconds.

Author: Wigsont Y.G. Wang
Email: wigsont@126.com
Date: April 20, 2025

Overview
GLKB is a project for transforming raw geological label text into structured complex labels suitable for direct use in ArcGIS.
The code is developed in C# using .NET Framework 4.8, and requires Microsoft Access (.accdb) databases for both input and knowledge storage.

IDE: Visual Studio 2013
Language: C#
Project File: AutoComplexLabels.sln
Framework: .NET Framework 4.8

Key Components
GCR Class
The core class responsible for transforming raw text data from the database into structured geological complex labels automatically.

DBMS:Microsoft Access (.accdb) for managing both the knowledge base and raw data.

GLKB.accdb – Geological Label Knowledge Base
Contains three tables: Labeling Elements, VSM, and Characteristic Values
Notes:Field names in these tables are hardcoded in the source code. Any schema changes must be reflected in the code.

ComplexLabels.accdb – Raw Text Data
Contains a single table:Qu – Stores raw label text to be processed into complex geological labels.
Notes:Field names must remain in sync between the database and the C# source code.

User Interface:
A Windows Form named FormBatchProcess (titled "BatchConvert2ComplexLabelBatch") provides a simple UI for batch processing.
The form contains a single button: "Batch Process". Clicking the button initiates the automatic generation of complex geological labels.

Notes:
(a) Be cautious when modifying field names or database structure, as they are directly referenced in the source code.
(b) The DBAccessor class is a utility for reading data from database. It was originally created by another contributor and later modified by the author.
