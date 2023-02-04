Create database TaskManament

--Drop database TaskManament
Create table Task(
TaskID varchar(16) not null,
TaskTitle Nvarchar(100) not null,
PinTask bit not null ,
SubTask varchar(16) not null,
Primary key (TaskID)
)

Create table TypeTask(
TypeTaskID varchar(10) not null,
Title Nvarchar(100) not null,
[Description] nvarchar(500)   null ,
Primary key (TypeTaskID)
)

Create table TaskDetail(
TaskID varchar(16) not null,
[Description] Nvarchar(500 ) null,
TimeCreated datetime not null ,
TimeLimited datetime not null ,
[Status] bit not null,
TypeTaskID varchar(10) not null,
Primary key (TaskID),
foreign key (TaskID) REFERENCES Task(TaskID),
foreign key (TypeTaskID) REFERENCES TypeTask(TypeTaskID),
)

Create table [User](
UserID varchar(10) not null,
Name nvarchar(30) not null,
Age int null,
Phone varchar(10) null,
Email varchar(40) not null,
Work nvarchar(30) not null,
Primary key (UserID)
)

Create table Account(
UserID varchar(10) not null,
UserName varchar(20) not null,
Passwork varchar(20) not null,
primary key(UserID,UserName),
foreign key (UserID) REFERENCES [User](UserID)
)

Create table TaskManament(
UserID varchar(10) not null,
TaskID varchar(16) not null,
Role Nvarchar(20) not null,
primary key (UserID,TaskID),
foreign key (UserID) REFERENCES [User](UserID),
foreign key (TaskID) REFERENCES Task(TaskID)
)