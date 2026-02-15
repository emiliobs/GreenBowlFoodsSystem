🥗 Green Bowl Foods System

Inventory & Quality Management System – Web-Based Prototype

📌 Project Overview

The Green Bowl Foods System is a full-stack web-based management application designed to support food manufacturing operations. The system centralizes inventory control, production tracking, quality assurance, logistics, and financial records into a single secure platform.

This project was developed as part of the CC6012 Coursework Project for the BSc (Hons) Computing programme and demonstrates the practical application of modern software engineering principles in a real-world business context.

🎯 Key Features

Secure authentication and role-based access control (Admin / Staff)

Full CRUD functionality across all operational modules

Inventory management for raw materials, packaging, and finished products

Production batch tracking with material consumption and stages

Quality assurance logging, including X-Ray inspections

Distribution management with shipments and delivery verification

Financial invoicing with status tracking (Paid, Unpaid, Overdue)

Dashboards and reports for operational visibility

Print-friendly reports and audit-ready records

🛠 Technologies Used

.NET 10

ASP.NET Core MVC

Entity Framework Core (Code-First)

SQL Server

ASP.NET Core Identity

Bootstrap 5

Bootstrap Icons

DataTables

SweetAlert2

Visual Studio 2026

🚀 Live Demo

🔗 Live System (MVP Prototype):
https://greenbowlfoodssystem.runasp.net/

🔐 Demo Credentials

Admin User: admin@yopmail.com

Password: 123

These credentials are provided for demonstration and assessment purposes only.

📂 Repository Contents

This repository includes:

ASP.NET Core MVC source code

Entity Framework Core models and migrations

Database seeding logic for demo data

MVC Controllers and Views

UI styling and client-side scripts

Coursework documentation and diagrams

📊 System Modules

Users & Roles (Admin only)

Inventory

Receiving Forms

Raw Materials

Packaging Materials

Suppliers

Production

Production Batches

Production Stages

X-Ray Quality Checks

Finished Products

Distribution & Sales

Shipments

Delivery Forms

Invoices

Reports

Inventory Valuation

Production Yield

Quality Assurance

Sales & Logistics

🧪 Testing & Data

Pre-seeded demo data for all modules

Edge cases included (low stock, expired materials, failed inspections, overdue invoices)

Manual functional testing was performed on all CRUD operations

Role-based access and validation tested across modules

👨‍🎓 Academic Information

Student: Emilio Antonio Barrera Sepúlveda

Student ID: 22047090

Course: BSc (Hons) Computing

Module: CC6012

Academic Year: 2025–2026

University: London Metropolitan University

📜 License & Disclaimer

This system was developed for academic purposes only.
All data used in the live system is fictional and intended solely for demonstration and assessment.

📸 Screenshots

The following screenshots illustrate key features and user interfaces of the Green Bowl Foods System, demonstrating how the application supports inventory management, production tracking, quality assurance, distribution, and financial operations.

🔐 Authentication & Access Control
## <img width="2542" height="1351" alt="image" src="https://github.com/user-attachments/assets/cab45436-0c48-46e0-b4e8-51218ca2a2ec" />


Login interface secured with ASP.NET Core Identity. Role-based access ensures that only authorized users can access system modules.

📊 Dashboard (Post-Login Overview)

Operational dashboard providing real-time visibility of inventory status, production batches, quality alerts, shipments, and financial indicators.

👥 System Users Management (Admin Only)

System Users Index view showing role badges, search, pagination, and administrative actions.

📦 Inventory Management

Raw Materials Index view displaying stock levels, expiry dates, and low-stock indicators.

Receiving Forms module used to register incoming deliveries and inspection results.

⚙️ Production & Quality Control

Production Batches view showing batch status (Planned, In Progress, Completed, QA Hold).

X-Ray Quality Checks module used to log inspection results and ensure food safety compliance.

🚚 Distribution & Sales

Shipments module tracking outgoing deliveries, carriers, and delivery status.

Invoices Index view displaying financial records with status indicators (Paid, Unpaid, Overdue).

📈 Reports Module

Reports module providing read-only analytical views for inventory valuation, production yield, quality assurance, sales, and logistics.
