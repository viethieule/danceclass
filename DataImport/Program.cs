using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.IO;
using DataAccess.Entities;
using DataAccess;
using Services.Utils;
using System.Text.RegularExpressions;
using System.Data.Entity;
using DataAccess.IdentityAccessor;

namespace DataImport
{
    public class Program
    {
        static void Main(string[] args)
        {
            _Application app = new Application();
            Workbook book = null;
            Worksheet sheet = null;
            Range range = null;

            try
            {
                app.Visible = false;
                app.ScreenUpdating = false;
                app.DisplayAlerts = false;

                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"MasterSheet.xlsx");

                book = app.Workbooks.Open(filePath, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                sheet = (Worksheet)book.Worksheets[1];

                // get a range to work with
                range = sheet.get_Range("A2", Missing.Value);
                // get the end of values to the right (will stop at the first empty cell)
                range = range.get_End(XlDirection.xlToRight);
                // get the end of values toward the bottom, looking in the last column (will stop at first empty cell)
                range = range.get_End(XlDirection.xlDown);

                // get the address of the bottom, right cell
                string downAddress = range.get_Address(
                    false, false, XlReferenceStyle.xlA1,
                    Type.Missing, Type.Missing);

                // Get the range, then values from a1
                range = sheet.get_Range("A2", downAddress);
                object[,] values = (object[,])range.Value2;

                // Value2 is a two dimenial array dime one = row, dime two = column.
                Console.WriteLine("Col Count: " + values.GetLength(1).ToString());
                Console.WriteLine("Row Count: " + values.GetLength(0).ToString());

                using (DanceClassDbContext db = new DanceClassDbContext())
                {
                    var users = new List<ApplicationUser>();
                    var memberships = new List<Membership>();
                    var packages = new List<Package>();

                    var defaultPackages = db.DefaultPackages.ToList();

                    for (int i = 1; i <= values.GetLength(0); i++)
                    {
                        string fullName = values[i, 1].ToString().Trim();
                        if (!values[i, 2].ToString().Equals("0"))
                        {
                            fullName = values[i, 2].ToString().Trim() + " " + fullName;
                        }

                        DateTime? dob = !values[i, 3].Equals("undefined") ? (DateTime?)DateTime.FromOADate((double)values[i, 3]) : null;
                        string phoneNumber = "0" + values[i, 4].ToString().Trim();

                        int numberOfSessions = int.Parse(values[i, 5].ToString());
                        int months = int.Parse(values[i, 6].ToString());
                        DateTime createdDate = DateTime.FromOADate((double)values[i, 7]);
                        DateTime expiryDate = DateTime.FromOADate((double)values[i, 8]);
                        int remainingSessions = int.Parse(values[i, 10].ToString());
                        double price = (double)values[i, 11];

                        ApplicationUser user = users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
                        Membership membership = memberships.FirstOrDefault(m => m.User.PhoneNumber == phoneNumber);

                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                FullName = fullName,
                                UserName = GenerateUserName(users, fullName),
                                PhoneNumber = phoneNumber,
                                Birthdate = dob,
                                CreatedBy = "Imported",
                                UpdatedBy = "Imported",
                                CreatedDate = createdDate,
                                UpdatedDate = createdDate
                            };

                            membership = new Membership
                            {
                                User = user,
                                RemainingSessions = remainingSessions,
                                ExpiryDate = expiryDate,
                                CreatedBy = "Imported",
                                UpdatedBy = "Imported",
                                CreatedDate = createdDate,
                                UpdatedDate = createdDate
                            };

                            users.Add(user);
                            memberships.Add(membership);
                        }
                        else
                        {
                            membership.RemainingSessions += remainingSessions;
                            membership.ExpiryDate = membership.ExpiryDate > expiryDate ? membership.ExpiryDate : expiryDate;
                        }

                        DefaultPackage defaultPackage = defaultPackages.FirstOrDefault(p => p.NumberOfSessions == numberOfSessions);
                        Package package = new Package
                        {
                            NumberOfSessions = numberOfSessions,
                            RemainingSessions = remainingSessions,
                            Months = months,
                            Price = price,
                            DefaultPackageId = defaultPackage != null ? (int?)defaultPackage.Id : null,
                            User = user,
                            CreatedBy = "Imported",
                            UpdatedBy = "Imported",
                            CreatedDate = createdDate,
                            UpdatedDate = createdDate,
                            ExpiryDate = expiryDate
                        };
                        packages.Add(package);
                    }

                    ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(db));
                    using (DbContextTransaction transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            Console.WriteLine("--------------------------------------");
                            Console.WriteLine("- ADDING USERS -------------------------");
                            Console.WriteLine("--------------------------------------");
                            foreach (var u in users)
                            {
                                Console.WriteLine("Adding user: " + u.FullName);
                                var result = userManager.CreateAsync(u, "Mistake1234").GetAwaiter().GetResult();
                                if (result.Succeeded)
                                {
                                    result = userManager.AddToRoleAsync(u.Id, "Member").GetAwaiter().GetResult();
                                }

                                if (!result.Succeeded)
                                {
                                    throw new Exception(string.Join(". ", result.Errors));
                                }
                            }

                            Console.WriteLine("--------------------------------------");
                            Console.WriteLine("- ADDING MEMBERSHIPS -------------------------");
                            Console.WriteLine("--------------------------------------");
                            db.Memberships.AddRange(memberships);

                            Console.WriteLine("--------------------------------------");
                            Console.WriteLine("- ADDING PACKAGES -------------------------");
                            Console.WriteLine("--------------------------------------");
                            db.Packages.AddRange(packages);

                            Console.WriteLine("--------------------------------------");
                            Console.WriteLine("- SAVING CHANGES -------------------------");
                            Console.WriteLine("--------------------------------------");
                            db.SaveChanges();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                range = null;
                sheet = null;
                if (book != null)
                {
                    try
                    {
                        book.Close(false, Missing.Value, Missing.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                book = null;
                if (app != null)
                {
                    try
                    {
                        app.Quit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                    
                app = null;
                Console.ReadKey();
            }
        }

        private static string GenerateUserName(List<ApplicationUser> users, string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            fullName = StringHelper.ConverToUnsignedString(fullName.Trim());
            string[] names = Regex.Split(fullName.ToLower(), @"\s+");

            if (names.Length == 1)
            {
                string userName = names[0];

                List<string> existingUsernames = GetUsernamesStartWith(users, userName);
                int numberOfExistingUsernames = existingUsernames.Count(u =>
                {
                    string[] part = u.Split('.');
                    return part[0] == userName &&
                        (part.Length == 1 || (part.Length == 2 && int.TryParse(part[1], out _)));
                });

                if (numberOfExistingUsernames == 0)
                {
                    return userName;
                }
                else
                {
                    return userName + "." + numberOfExistingUsernames;
                }
            }
            else
            {
                string userName = names[names.Length - 1] + "." + names[0];

                List<string> existingUsernames = GetUsernamesStartWith(users, userName);
                int numberOfExistingUsernames = existingUsernames.Count(u =>
                {
                    string[] part = u.Split('.');
                    return part[0] + "." + part[1] == userName;
                });

                if (numberOfExistingUsernames == 0)
                {
                    return userName;
                }
                else
                {
                    return userName + "." + numberOfExistingUsernames;
                }
            }
        }

        private static List<string> GetUsernamesStartWith(List<ApplicationUser> users, string start)
        {
            return users
                .Where(u => u.UserName.StartsWith(start))
                .Select(u => u.UserName)
                .ToList();
        }
    }
}