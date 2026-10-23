// Minimal console runner for the EditMode tests: lets the pure C# gameplay tests run outside Unity
// (e.g. while the editor has the project open). Supports [SetUp], [Test] and [TestCase].
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

public static class Runner
{
    public static int Main()
    {
        int passed = 0, failed = 0;
        var testTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "VertigoCase.Tests.EditMode");

        foreach (Type type in testTypes)
        {
            MethodInfo setUp = type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<SetUpAttribute>() != null);
            foreach (MethodInfo method in type.GetMethods())
            {
                var cases = method.GetCustomAttributes<TestCaseAttribute>().Select(c => c.Arguments).ToList();
                if (method.GetCustomAttribute<TestAttribute>() != null) cases.Add(Array.Empty<object>());

                foreach (object[] args in cases)
                {
                    string name = $"{type.Name}.{method.Name}({string.Join(", ", args)})";
                    object instance = Activator.CreateInstance(type);
                    try
                    {
                        setUp?.Invoke(instance, null);
                        object[] converted = method.GetParameters()
                            .Select((p, i) => p.ParameterType.IsEnum ? Enum.ToObject(p.ParameterType, args[i]) : Convert.ChangeType(args[i], p.ParameterType))
                            .ToArray();
                        method.Invoke(instance, converted);
                        passed++;
                    }
                    catch (TargetInvocationException e)
                    {
                        failed++;
                        Console.WriteLine($"FAIL {name}: {e.InnerException?.Message}");
                    }
                }
            }
        }

        Console.WriteLine($"{passed} passed, {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}
