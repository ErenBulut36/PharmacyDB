=============================================================================
PROJECT NAME: Pharmacy Automation
AUTHOR: Eren Bulut
=============================================================================

HOW TO RUN THIS PROJECT? (INSTALLATION GUIDE)

The source code and the database backup file (.bak) are provided in the 
project folder. Please follow the steps below to run the application on a 
new machine.

1. PREREQUISITES
-----------------------------------------------------------------------------
- Visual Studio 2022 (C# Windows Forms )
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)


STEP 1: DATABASE SETUP Restoring via .bak File
-----------------------------------------------------------------------------
The "PharmacyDB.bak" file contains the full database backup.

1. Open SQL Server Management Studio (SSMS) and connect to your instance.
2. Right-click on the "Databases" folder in the Object Explorer and select 
   "Restore Database...".
3. Select "Device", click the "..." button.
4. Click "Add", navigate to the project folder, and select "PharmacyDB.bak".
5. Click "OK" to finish the restoration process.
   (You should see a "Database 'PharmacyDB' restored successfully" message.)


STEP 2: CONFIGURING THE CONNECTION STRING
-----------------------------------------------------------------------------
Since every computer has a unique SQL Server Instance Name, you must update 
the connection string in the C# code to match your local environment.

1. Open "HomeScreen.sln" in Visual Studio.
2. Go to the code view of the Form files (e.g., PharmacistPanelForm.cs).
3. Locate the connection string line at the top of the class:
(the connection I have)
   string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

4. Replace "Server=..." with your own SQL Server name.
   (You can find your server name in the "Server name" field when launching SSMS.)

"ATTENTION!!!: You must make this change in ALL forms (AdminPanelForm, AdminForm, DoctorForm, etc.) that use a database connection in the project."

   Example Update:
   string connectionString = "Server=YOUR_PC_NAME\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";


STEP 3: RUNNING THE APPLICATION
-----------------------------------------------------------------------------
- Press the "Start" button in Visual Studio to launch the application.
- You can use the following pre-registered credentials to test the system:

   [Admin Login]
   Username: admin1
   Password: 1234

   [Doctor Login]
   Username: dr_buğrahan
   Password: 1234

   [Pharmacist Login]
   Username: ph_eren
   Password: 1234