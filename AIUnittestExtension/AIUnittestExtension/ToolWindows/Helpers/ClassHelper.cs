using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.CSharp.Formatting;

namespace AIUnittestExtension.ToolWindows.Helpers
{
    public static class ClassHelper

    {
        public static string ExtractClassName(string filePath)
        {
            var input = File.ReadAllText(filePath);
            Match match = Regex.Match(input, @"\bclass\s+(\w+)", RegexOptions.Singleline);
            if (!match.Success) return null;

            return match.Groups[1].Value;
        }
        public static List<string> ExtractPublicMethods(string filePath)
        {
            //use Roslyn
            // Parse the C# code into a syntax tree
            var code = File.ReadAllText(filePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = (CompilationUnitSyntax)tree.GetRoot();

            // Extract eligible methods
            var eligibleMethods = ExtractEligibleMethods(root);

            return eligibleMethods.Select(m => m.ToFullString()).ToList();
        }
        static List<MethodDeclarationSyntax> ExtractEligibleMethods(CompilationUnitSyntax root)
        {
            var eligibleMethods = new List<MethodDeclarationSyntax>();

            // Extract all method declarations in the file
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                // Check if method is eligible based on the criteria:
                if (IsEligibleForUnitTest(method))
                {
                    eligibleMethods.Add(method);
                }
            }

            return eligibleMethods;
        }

        static bool IsEligibleForUnitTest(MethodDeclarationSyntax method)
        {
            // 1. The method should have a body
            if (method.Body == null) return false;

            // 2. The method should be public
            if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) == false) return false;

            // 3. The method should not be a simple getter/setter
            if (method.Body.Statements.Count == 1 &&
                method.Body.Statements[0] is ReturnStatementSyntax returnStmt &&
                returnStmt.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == method.Identifier.Text)
            {
                return false;  // This is likely a getter or setter, not a testable method
            }

            return true;
        }
        public static string ExtractClassTemplate(string filePath, string token)
        {
            var input = File.ReadAllText(filePath);

            // Extract all using statements
            string usingPattern = @"^\s*using\s+[A-Za-z0-9_.]+;\s*$";
            List<string> usingMatches = Regex.Matches(input, usingPattern, RegexOptions.Multiline)
                                     .Cast<Match>()  // Explicitly cast MatchCollection to IEnumerable<Match>
                                     .Select(m => m.Value.Trim()) // Extract value and trim spaces
                                     .ToList(); // Convert to List<string>

            string usings = string.Join("\n", usingMatches);
            // Extract namespace declaration
            Match namespaceMatch = Regex.Match(input, @"\bnamespace\s+[\w.]+\s*{", RegexOptions.Singleline);
            string namespaceDeclaration = namespaceMatch.Success ? namespaceMatch.Value : "";

            // Regex to match class declaration (with or without inheritance)
            Match match = Regex.Match(input, @"\bclass\s+(\w+)", RegexOptions.Singleline);
            if (!match.Success) return null;

            var className = match.Groups[1].Value;

            var template = usings + "\n";
            template += "using Xunit;\n";
            template += "using Moq;\n\n";
            template += namespaceDeclaration + "\n";
            template += $"    public class {className}Test\n";
            template += "    {\n";
            template += $"{token}\n";
            template += "    }\n";
            template += "}\n";

            return template;
        }
        public static List<string> ExtractPublicMethodsRegex(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            List<string> methodChunks = new List<string>();
            StringBuilder currentMethod = new StringBuilder();
            bool insideMethod = false;
            int openBraces = 0;

            //Regex to match multi-line method signatures and ensure it's public
            Regex methodSignatureRegex = new Regex(@"^\s*(public|protected|internal)\s+(static\s+)?(async\s+)?([\w<>\.]+)\s+\w+\s*\(.*", RegexOptions.Compiled);


            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                //Detect method signature (allowing multi-line signatures)
                if (!insideMethod && methodSignatureRegex.IsMatch(trimmedLine))
                {
                    insideMethod = true;
                    openBraces = 0;
                    currentMethod.Clear();
                }

                if (insideMethod)
                {
                    currentMethod.AppendLine(line);

                    //Start counting braces **only after the first `{` appears**
                    if (trimmedLine.Contains("{"))
                    {
                        openBraces += CountOccurrences(trimmedLine, '{');
                    }
                    if (trimmedLine.Contains("}"))
                    {
                        openBraces -= CountOccurrences(trimmedLine, '}');
                    }

                    //If all `{` are matched with `}`, method is fully captured
                    if (openBraces == 0 && trimmedLine.Contains("}"))
                    {
                        methodChunks.Add(currentMethod.ToString().Trim()); // Store full method
                        insideMethod = false;
                    }
                }
            }

            return methodChunks;
        }
        public static Dictionary<int, List<T>> Bucketize<T>(IEnumerable<T> source, Func<T, int> getItemSize, int maxBucketSize, int minItemSize = 1)
        {
            if (source == null || !source.Any())
                return new Dictionary<int, List<T>>();
            var buckets = new Dictionary<int, List<T>>
            {
                [1] = new List<T>()
            };
            var currentPage = 1;
            var currentBucket = buckets[currentPage];
            var currentBucketSize = 0;
            if (getItemSize == null)
                getItemSize = _ => 1;

            foreach (var item in source)
            {
                var itemSize = getItemSize(item);
                if (minItemSize > itemSize)
                    itemSize = minItemSize;
                if (itemSize > maxBucketSize)
                    throw new ArgumentException($"Found method line of {itemSize} line, but the max method size is {maxBucketSize}");

                if (currentBucketSize + itemSize > maxBucketSize)
                {
                    currentPage++;
                    currentBucket = buckets[currentPage] = new List<T>();
                    currentBucketSize = 0;
                }

                currentBucket.Add(item);
                currentBucketSize += itemSize;
            }

            return buckets;
        }
        static int CountOccurrences(string text, char character)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == character) count++;
            }
            return count;
        }

        public static string FormatCSharpCode(string code)
        {
            // Parse the code into a syntax tree
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = tree.GetRoot();

            // Normalize the whitespace (fixes indentation & spacing)
            SyntaxNode formattedNode = root.NormalizeWhitespace();

            return formattedNode.ToFullString();
        }
    }
}
