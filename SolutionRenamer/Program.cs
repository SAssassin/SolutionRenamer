using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SolutionRenamer
{
    static class Program
    {
        /// <summary>
        /// 主程序
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "SolutionRenamer";

            //加载配置
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config.json");

            //加载文件后缀名
            var configuration = builder.Build();
            var fileExtensions = configuration["FileExtension"];
            string[] filter = fileExtensions.Split(',');

            //Console.WriteLine("Input your old company name:");
            //var oldCompanyName = Console.ReadLine();
            var oldCompanyName = string.Empty;
            Console.WriteLine("Input your old project name:");
            var oldProjectName = Console.ReadLine();

            //Console.WriteLine("Input your new company name:");
            //var newCompanyName = Console.ReadLine();
            var newCompanyName = string.Empty;
            Console.WriteLine("Input your new project name:");
            var newProjectName = Console.ReadLine();

            //项目根路径
            Console.WriteLine("Input folder:" + System.AppDomain.CurrentDomain.BaseDirectory);
            var rootDir = Console.ReadLine();

            Stopwatch sp = new Stopwatch();
            sp.Start();
            RenameAllDir(rootDir, oldCompanyName, oldProjectName, newCompanyName, newProjectName);
            sp.Stop();
            long spdir = sp.ElapsedMilliseconds;
            Console.WriteLine("Directory rename complete! spend:" + sp.ElapsedMilliseconds);

            sp.Reset();
            sp.Start();
            RenameAllFileNameAndContent(rootDir, oldCompanyName, oldProjectName, newCompanyName, newProjectName, filter);
            sp.Stop();
            long spfile = sp.ElapsedMilliseconds;
            Console.WriteLine("Filename and content rename complete! spend:" + sp.ElapsedMilliseconds);

            Console.WriteLine("");
            Console.WriteLine("=====================================Report=====================================");
            Console.WriteLine($"Processing spend time,directories:{spdir},files:{spfile}");
            Console.ReadKey();
        }

        /// <summary>
        /// 递归重命名所有目录
        /// </summary>
        static void RenameAllDir(string rootDir, string oldCompanyName, string oldProjectName, string newCompanyName, string newProjectName)
        {
            string[] allDir = Directory.GetDirectories(rootDir);

            foreach (var item in allDir)
            {
                RenameAllDir(item, oldCompanyName, oldProjectName, newCompanyName, newProjectName);

                DirectoryInfo dinfo = new DirectoryInfo(item);
                if (dinfo.Name.Contains(oldCompanyName) || dinfo.Name.Contains(oldProjectName))
                {
                    var newName = dinfo.Name;

                    if (!string.IsNullOrEmpty(oldCompanyName))
                    {
                        newName = newName.Replace(oldCompanyName, newCompanyName);
                    }
                    if (!string.IsNullOrEmpty(oldProjectName))
                    {
                        newName = newName.Replace(oldProjectName, newProjectName);
                    }
                    var newPath = Path.Combine(dinfo.Parent.FullName, newName);

                    if (dinfo.FullName != newPath)
                    {
                        Console.WriteLine(dinfo.FullName);
                        Console.WriteLine("->");
                        Console.WriteLine(newPath);
                        Console.WriteLine("-------------------------------------------------------------");
                        dinfo.MoveTo(newPath);
                    }
                }
            }
        }

        /// <summary>
        /// 递归重命名所有文件名和文件内容
        /// </summary>
        static void RenameAllFileNameAndContent(string rootDir, string oldCompanyName, string oldProjectName, string newCompanyName, string newProjectName, string[] filter)
        {
            //获取当前目录所有指定文件扩展名的文件
            List<FileInfo> files = new DirectoryInfo(rootDir).GetFiles().Where(m => filter.Any(f => f == m.Extension)).ToList();

            //重命名当前目录文件和文件内容
            foreach (var item in files)
            {
                var text = File.ReadAllText(item.FullName, Encoding.UTF8);
                if (!string.IsNullOrEmpty(oldCompanyName))
                {
                    text = text.Replace(oldCompanyName, newCompanyName);
                }

                if (!string.IsNullOrEmpty(oldProjectName))
                {
                    text = text.Replace(oldProjectName, newProjectName);
                }

                if (item.Name.Contains(oldCompanyName) || item.Name.Contains(oldProjectName))
                {
                    var newName = item.Name;

                    if (!string.IsNullOrEmpty(oldCompanyName))
                    {
                        newName = newName.Replace(oldCompanyName, newCompanyName);
                    }

                    if (!string.IsNullOrEmpty(oldProjectName))
                    {
                        newName = newName.Replace(oldProjectName, newProjectName);
                    }

                    var newFullName = Path.Combine(item.DirectoryName, newName);
                    File.WriteAllText(newFullName, text, Encoding.UTF8);
                    if (newFullName != item.FullName)
                    {
                        File.Delete(item.FullName);
                    }
                }
                else
                {
                    File.WriteAllText(item.FullName, text, Encoding.UTF8);
                }
                Console.WriteLine(item.Name + " process complete!");
            }

            //获取子目录
            string[] dirs = Directory.GetDirectories(rootDir);
            foreach (var dir in dirs)
            {
                RenameAllFileNameAndContent(dir, oldCompanyName, oldProjectName, newCompanyName, newProjectName, filter);
            }
        }
    }

}
