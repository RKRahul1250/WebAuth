
# WebAuth - Secure Authentication System

A robust authentication system built with ASP.NET Core 9.0, featuring secure password management and interactive UI elements.

## Live Demo
[Visit WebAuth](https://rkrahul1250.github.io/WebAuth/)

## Features
- Secure User Authentication
- Admin Dashboard
- Password Encryption with Salt
- Interactive UI with Audio Feedback
- User Management System
- Responsive Design

## Tech Stack
- ASP.NET Core 9.0
- Bootstrap 5
- Custom Authentication Service
- Web Audio API for Interactive Feedback
- Secure Password Hashing

## Data Storage
- User credentials are stored in a secure text file (users.txt)
- Passwords are never stored in plain text
- Each password is:
  - Salted with a unique value
  - Encrypted using secure hashing
  - Stored in format: username:salt:hashedPassword
- Admin can manage users through dashboard
- File-based storage for easy deployment and testing
