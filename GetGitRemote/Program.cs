using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace GetGitRemote
{
    class Program
    {
        private static readonly Regex parseOrigin = new Regex( @"remote\.(?<remote>\w+)\.url" );
        private static readonly Regex parseSshRepo = new Regex( @"git@github\.com:(?<repo>\w+/\w+)\.git" );
        private static readonly Regex parseHttpsRepo = new Regex( @"https://github.com/(?<repo>\w+/\w+)\.git" );

        static void Main( string[] args )
        {
            string path = args.Length == 1 ? args[0] : GetWorkingDirectory();

            path = FindRootOfRepository( path );
            if ( path != null )
            {
                // Need to walk up the directory structure until we find a .git directory
                InspectRepository( path );
            }
            else
            {
                Console.WriteLine( "This is not a Git repository" );
            }

            Console.WriteLine();
            Console.WriteLine( "*** Press ENTER to Exit ***" );
            Console.ReadLine();
        }

        private static string FindRootOfRepository( string path )
        {
            var dir = new DirectoryInfo( path );
            if ( !dir.Exists ) return null;

            while ( dir != null )
            {
                if ( dir.EnumerateDirectories( ".git" ).Any( ) ) return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        private static void InspectRepository( string path )
        {
            using ( var repo = new Repository( path ) )
            {
                Console.WriteLine( "BRANCHES" );
                Console.WriteLine( );
                foreach ( var branch in repo.Branches )
                {
                    if ( branch.Remote == null )
                    {
                        Console.WriteLine( branch.Name );
                    }
                    else
                    {
                        Console.WriteLine( "{0} -> {1} -> {2}", branch.Name, branch.Remote.Name, branch.Remote.Url );
                    }
                }

                Console.WriteLine( );
                Console.WriteLine( "CONFIG" );
                Console.WriteLine( );
                foreach ( var config in repo.Config )
                {
                    var origin = parseOrigin.Match( config.Key );
                    if ( origin.Success )
                    {
                        var githubRepo = GetRemoteGitHubRepository( config.Value );
                        if ( githubRepo != null )
                        {
                            Console.WriteLine( "{0} = {1}", origin.Groups["remote"].Value, githubRepo );
                        }
                    }
                }
            }
        }

        private static string GetRemoteGitHubRepository( string repoUri )
        {
            var ssh = parseSshRepo.Match( repoUri );
            if ( ssh.Success ) return ssh.Groups["repo"].Value;

            var https = parseHttpsRepo.Match( repoUri );
            if ( https.Success ) return https.Groups["repo"].Value;

            return null;
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
