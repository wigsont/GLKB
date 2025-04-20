# GLKB
C# Source Code for Generating Geological Complex Labels Using a Knowledge Base

This project provides C# source code for automatically generating geological complex labels, assisted by a domain-specific knowledge base. Each label is generated in approximately 3 milliseconds.

Author: Wigsont Y.G. Wang
Date: April 20, 2025

The project was developed using Visual Studio 2013, and the solution file is named AutoComplexLabels.sln.

Overview
The core functionality is implemented in C# and relies on Microsoft Access databases (.accdb) for both input data and the geological knowledge base.

Key Components
GCR Class
Main class responsible for transforming raw database text fields into structured geological complex labels automatically.

DBAccessor Class
Handles reading data from Microsoft Access database fields.

Database System

Uses Microsoft Access (.accdb) format.

Two main databases are involved:

**GLKB.accdb**: Geological label knowledge base.

Tables: Labeling Elements, VSM, Characteristic Values.

Note: Field names are hard-coded in the source code and must be updated in sync with the database schema.

**ComplexLabels.accdb**: Contains raw text data for labels.

Table: Qu (user-editable for raw label inputs).

Again, field names must remain consistent with the code.

Form UI

A Windows Form named FormBatchProcess (titled "BatchConvert2ComplexLabelBatch") is provided.

It includes a single button labeled "Batch Process" that triggers the automatic generation of complex geological labels.

Notes
Be cautious when modifying field names in the database — these are directly referenced in the C# source code.

The solution is designed for domain experts and developers working with geological data labeling and requires Microsoft Access to manage the data sources.
