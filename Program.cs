using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ddd_øvelser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Building");
            var fabric = new Factory(LocationNames.Fabric1);
            var port = new Location(LocationNames.Port1);
            var targetA = new Location(LocationNames.A);
            var targetB = new Location(LocationNames.B);
            var truckAllowFrom = new List<ILocation> { fabric, port, targetB };
            var truckAllowTo = new List<ILocation> { fabric, port, targetB };
            var shipAllowFrom = new List<ILocation> { port, targetA };
            var shipAllowTo = new List<ILocation> { port, targetA };
            var truck1 = new Truck { AllowedFrom = truckAllowFrom, AllowedTo = truckAllowTo };
            var truck2 = new Truck { AllowedFrom = truckAllowFrom, AllowedTo = truckAllowTo };
            var ship1 = new Ship { AllowedFrom = shipAllowFrom, AllowedTo = shipAllowTo };
            var route1 = new RouteStep { From = fabric, To = targetB, Duration = 5 };
            var route2 = new RouteStep { From = fabric, To = port, Duration = 1 };
            var route3 = new RouteStep { From = port, To = targetA, Duration = 4 };


            var containers = new List<LocationNames> { LocationNames.A, LocationNames.B };
            containers.ForEach(x => fabric.AddContainer(new Container { TargetName = x }));

            fabric.Estimate(new List<RouteStep> { route1, route2, route3 });



        }
    }

    public class Truck : TransportDevice
    {

    }
    public class Ship : TransportDevice
    {
    }
    public class Factory : ILocation
    {
        public Factory(LocationNames name) { Name = name; Stock = new List<Container>(); }
        public LocationNames Name { get; private set; }
        public List<Container> Stock { get; set; }


        public void AddContainer(Container container)
        {
            Stock.Add(container);
        }

        public List<Route> FindPossibleRoutes(LocationNames from, Route currentRoute, LocationNames target, List<RouteStep> allSteps)
        {
            var result = new List<Route>();
            var possibleSteps = allSteps.Where(x => x.From.Name == from).ToList();
            if (!possibleSteps.Any())
            {
                Debug.WriteLine("   - not possible route");
                return result;
            }
            var match = possibleSteps.Where(x => x.To.Name == target);
            if (match.Any())
            {
                // We have a match
                match.ToList().ForEach(x =>
                {
                    var newRouteSteps = new List<RouteStep>();
                    currentRoute.Steps.ForEach(s => newRouteSteps.Add(s));
                    newRouteSteps.Add(x);
                    var matchedRoute = new Route { Steps = newRouteSteps };
                    Debug.WriteLine("   - Found match - " + matchedRoute.ToString());
                    result.Add(matchedRoute);
                });
            }
            else
            {
                // Search again, form current step
                possibleSteps.ForEach(x =>
                {
                    var newRouteSteps = new List<RouteStep>();
                    currentRoute.Steps.ForEach(s => newRouteSteps.Add(s));
                    newRouteSteps.Add(x);
                    var possibleRoute = new Route { Steps = newRouteSteps };
                    Debug.WriteLine($"Checking out possible route: {possibleRoute}");
                    var possibleRoutes = FindPossibleRoutes(x.To.Name, possibleRoute, target, allSteps);
                    result.AddRange(possibleRoutes);
                });
            }
            return result;
        }

        public void Estimate(List<RouteStep> allSteps)
        {
            // Build routes
            Stock.ForEach(c =>
            {
                Debug.WriteLine("Estimating container heading for: " + c.TargetName);
                var routes = FindPossibleRoutes(Name, new Route { Steps = new List<RouteStep> { } }, c.TargetName, allSteps);
                routes.ForEach(r =>
                {
                    Debug.WriteLine("   - Found route", r.ToString());
                });
            });
        }
    }
    public class Location : ILocation
    {
        public Location(LocationNames name) { Name = name; }
        public LocationNames Name { get; private set; }
        public List<Container> Stock { get; set; }

    }
    public class Container
    {
        public LocationNames TargetName { get; set; }
    }

    public class Route
    {
        public List<RouteStep> Steps { get; set; }
        public override string ToString() {
            return $"Route ({Steps.Count} steps): " + string.Join(",", Steps.Select(x => x.ToString()));
        }
    }
    public class RouteStep
    {

        public int Duration { get; set; }
        public ILocation From { get; set; }
        public ILocation To { get; set; }

        public override string ToString(){
            return $"{From.Name}-{To.Name}[{Duration} hours]";
        }

    }
    public class TransportDevice
    {
        public int MaxCount => 1;
        public List<Container> Containers { get; set; }

        public List<ILocation> AllowedFrom { get; set; }
        public List<ILocation> AllowedTo { get; set; }
        public ILocation Destination { get; set; }
        public List<RouteStep> Route { get; set; }
    }
    public interface ILocation
    {
        public LocationNames Name { get; }
        public List<Container> Stock { get; set; }
    }

    public enum LocationNames
    {
        Fabric1,
        Port1,
        A,
        B,
    }

}
