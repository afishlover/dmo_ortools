// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using ManPower;

Data d = new Data();
d.Populate();
Console.WriteLine(d.ToString());
Problem p = new Problem(d);
p.Setup();
p.VariableDefinition();
p.ConstraintDefinition();
p.Solve();