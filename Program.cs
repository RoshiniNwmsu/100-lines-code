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

            // Load employees from the CSV on startup
            EmployeeRepository.LoadFromCsv(csvFilePath);

            bool exit = false;
            while (!exit)
            {
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

        static void AddEmployee()
        {
            Console.Write("Enter Employee ID: ");
            int id = ReadInt();

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

        static int ReadInt()
        {
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Invalid input. Enter a number: ");
            }
            return value;
        }

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

    class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }

        public void DisplayDetails()
        {
            Console.WriteLine($"ID: {Id}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Position: {Position}");
            Console.WriteLine($"Salary: {Salary:C}");
        }

        public string ToCsvLine()
        {
            return $"{Id},{Name},{Position},{Salary}";
        }

        public static Employee FromCsvLine(string line)
        {
            var parts = line.Split(',');

            // Defensive parsing for CSV lines
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

    static class EmployeeRepository
    {
        private static List<Employee> employees = new List<Employee>();

        public static void LoadFromCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("CSV file not found. Starting with an empty list.");
                return;
            }

            employees.Clear();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1)
            {
                Console.WriteLine("No employee data found in the CSV file.");
                return;
            }

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

        public static void SaveToCsv(string filePath)
        {
            var lines = new List<string>
            {
                "Id,Name,Position,Salary"
            };

            foreach (var emp in employees)
            {
                lines.Add(emp.ToCsvLine());
            }

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"Saved {employees.Count} employees to CSV.");
        }

        public static List<Employee> GetAllEmployees()
        {
            return employees;
        }

        public static Employee GetEmployeeById(int id)
        {
            return employees.Find(e => e.Id == id);
        }

        public static void AddEmployee(Employee emp)
        {
            employees.Add(emp);
        }

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
