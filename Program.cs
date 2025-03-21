using System;
using System.Collections.Generic;
using System.IO;

namespace EmployeeManagementCsvApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Employee Management System (CSV Version)");
            Console.WriteLine("----------------------------------------\n");

            string csvFilePath = "employees.csv";

            // Load existing employees from CSV when the program starts
            EmployeeRepository.LoadFromCsv(csvFilePath);

            bool exit = false;
            while (!exit)
            {
                // Display menu options
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Display All Employees");
                Console.WriteLine("2. Add Employee");
                Console.WriteLine("3. Find Employee by ID");
                Console.WriteLine("4. Remove Employee by ID");
                Console.WriteLine("5. Update Employee Salary");
                Console.WriteLine("6. Update Employee Name");
                Console.WriteLine("7. Save and Exit");

                Console.Write("Option: ");
                string choice = Console.ReadLine();
                Console.WriteLine();

                // Handle user's menu choice
                switch (choice)
                {
                    case "1":
                        DisplayAllEmployees();
                        break;
                    case "2":
                        AddEmployee();
                        break;
                    case "3":
                        FindEmployeeById();
                        break;
                    case "4":
                        RemoveEmployeeById();
                        break;
                    case "5":
                        UpdateEmployeeSalary();
                        break;
                    case "6":
                        UpdateEmployeeName();
                        break;
                    case "7":
                        // Save data and exit the program
                        EmployeeRepository.SaveToCsv(csvFilePath);
                        Console.WriteLine("Data saved! Exiting...");
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }
            }
        }

        // Display all employees in the system
        static void DisplayAllEmployees()
        {
            var employees = EmployeeRepository.GetAllEmployees();
            if (employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            Console.WriteLine("Employee List:");
            foreach (var emp in employees)
            {
                emp.DisplayDetails();
                Console.WriteLine("---------------------------");
            }
        }

        // Add a new employee
        static void AddEmployee()
        {
            Console.Write("Enter Employee ID: ");
            int id = ReadInt();

            // Prevent duplicate employee IDs
            if (EmployeeRepository.GetEmployeeById(id) != null)
            {
                Console.WriteLine("Employee with this ID already exists.");
                return;
            }

            Console.Write("Enter Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Position: ");
            string position = Console.ReadLine();

            Console.Write("Enter Salary: ");
            decimal salary = ReadDecimal();

            var newEmployee = new Employee
            {
                Id = id,
                Name = name,
                Position = position,
                Salary = salary
            };

            EmployeeRepository.AddEmployee(newEmployee);
            Console.WriteLine("Employee added successfully.");
        }

        // Find and display employee details by ID
        static void FindEmployeeById()
        {
            Console.Write("Enter Employee ID to search: ");
            int id = ReadInt();

            var emp = EmployeeRepository.GetEmployeeById(id);
            if (emp != null)
            {
                Console.WriteLine("Employee found:");
                emp.DisplayDetails();
            }
            else
            {
                Console.WriteLine("Employee not found.");
            }
        }

        // Remove an employee by their ID
        static void RemoveEmployeeById()
        {
            Console.Write("Enter Employee ID to remove: ");
            int id = ReadInt();

            bool removed = EmployeeRepository.RemoveEmployee(id);
            if (removed)
                Console.WriteLine("Employee removed.");
            else
                Console.WriteLine("Employee not found.");
        }

        // Update an employee's salary
        static void UpdateEmployeeSalary()
        {
            Console.Write("Enter Employee ID to update salary: ");
            int id = ReadInt();

            var emp = EmployeeRepository.GetEmployeeById(id);
            if (emp == null)
            {
                Console.WriteLine("Employee not found.");
                return;
            }

            Console.WriteLine($"Current Salary: {emp.Salary:C}");
            Console.Write("Enter new Salary: ");
            decimal newSalary = ReadDecimal();

            emp.Salary = newSalary;
            Console.WriteLine("Salary updated.");
        }

        // Update an employee's name
        static void UpdateEmployeeName()
        {
            Console.Write("Enter Employee ID to update name: ");
            int id = ReadInt();

            var emp = EmployeeRepository.GetEmployeeById(id);
            if (emp == null)
            {
                Console.WriteLine("Employee not found.");
                return;
            }

            Console.WriteLine($"Current Name: {emp.Name}");
            Console.Write("Enter new Name: ");
            string newName = Console.ReadLine();

            emp.Name = newName;
            Console.WriteLine("Name updated.");
        }

        // Safely read an integer input from the user
        static int ReadInt()
        {
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Invalid input. Enter a number: ");
            }
            return value;
        }

        // Safely read a decimal input from the user
        static decimal ReadDecimal()
        {
            decimal value;
            while (!decimal.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Invalid input. Enter a decimal number: ");
            }
            return value;
        }
    }

    // Represents an individual employee
    class Employee
    {
        public int Id { get; set; }             // Employee ID
        public string Name { get; set; }        // Employee Name
        public string Position { get; set; }    // Employee Position
        public decimal Salary { get; set; }     // Employee Salary

        // Display employee details on console
        public void DisplayDetails()
        {
            Console.WriteLine($"ID: {Id}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Position: {Position}");
            Console.WriteLine($"Salary: {Salary:C}");
        }

        // Convert employee object to CSV line format
        public string ToCsvLine()
        {
            return $"{Id},{Name},{Position},{Salary}";
        }

        // Create employee object from CSV line
        public static Employee FromCsvLine(string line)
        {
            var parts = line.Split(',');

            // Defensive coding: ensure there are exactly 4 parts
            if (parts.Length < 4)
                throw new Exception("Invalid CSV line format.");

            return new Employee
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                Position = parts[2],
                Salary = decimal.Parse(parts[3])
            };
        }
    }

    // Repository to manage the list of employees and CSV file operations
    static class EmployeeRepository
    {
        private static List<Employee> employees = new List<Employee>();

        // Load employee data from CSV file into memory
        public static void LoadFromCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("CSV file not found. Starting with an empty list.");
                return;
            }

            employees.Clear(); // Clear current list before loading fresh data
            var lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1)
            {
                Console.WriteLine("No employee data found in the CSV file.");
                return;
            }

            // Skip header line (i = 1)
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    var emp = Employee.FromCsvLine(lines[i]);
                    employees.Add(emp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line {i + 1}: {ex.Message}");
                }
            }

            Console.WriteLine($"Loaded {employees.Count} employees from CSV.");
        }

        // Save current employee list to CSV file
        public static void SaveToCsv(string filePath)
        {
            var lines = new List<string>
            {
                "Id,Name,Position,Salary" // CSV header
            };

            // Convert each employee to a CSV line
            foreach (var emp in employees)
            {
                lines.Add(emp.ToCsvLine());
            }

            // Write all lines to the file
            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"Saved {employees.Count} employees to CSV.");
        }

        // Get list of all employees
        public static List<Employee> GetAllEmployees()
        {
            return employees;
        }

        // Find an employee by ID
        public static Employee GetEmployeeById(int id)
        {
            return employees.Find(e => e.Id == id);
        }

        // Add a new employee to the list
        public static void AddEmployee(Employee emp)
        {
            employees.Add(emp);
        }

        // Remove an employee by ID
        public static bool RemoveEmployee(int id)
        {
            var emp = GetEmployeeById(id);
            if (emp != null)
            {
                employees.Remove(emp);
                return true;
            }
            return false;
        }
    }
}
