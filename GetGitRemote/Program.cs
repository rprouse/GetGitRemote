using System;
using System.IO;
using System.Reflection;
using LibGit2Sharp;

namespace GetGitRemote
{
    class Program
    {
        static void Main( string[] args )
        {
            string path = args.Length == 1 ? args[0] : GetWorkingDirectory();

            using ( var repo = new Repository( path ) )
            {
                Console.WriteLine( "BRANCHES" );
                Console.WriteLine();
                foreach ( var branch in repo.Branches )
                {
                    Console.WriteLine( "{0} -> {1} -> {2}", branch.Name, branch.Remote.Name, branch.Remote.Url );
                }

                Console.WriteLine( "CONFIG" );
                Console.WriteLine();
                foreach ( var config in repo.Config )
                {
                    if ( config.Key.StartsWith( "remote." ) && config.Key.EndsWith( ".url" ) )
                        Console.WriteLine( "{0} = {1}", config.Key, config.Value );
                }
            }
            Console.WriteLine( "*** Press ENTER to Exit ***" );
            Console.ReadLine();
        }

        /// <summary>
        /// Helper method to get the working directory
        /// </summary>
        public static string GetWorkingDirectory()
        {
            return Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) + Path.DirectorySeparatorChar;
        }
    }
}
