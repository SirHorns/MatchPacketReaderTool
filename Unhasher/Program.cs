// See https://aka.ms/new-console-template for more information

using ReplayNamesUnhasher;

var Unhasher = new Unhasher();
Unhasher.Initialize();


Console.WriteLine("Unhashing Packets");
try
{
    //Unhasher.Unhashie(slrf);
}
catch (Exception e)
{
    Console.WriteLine(e);
}