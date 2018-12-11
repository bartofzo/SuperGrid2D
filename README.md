# ContactGrid

A 2D Spatial Indexing library that utilizes a fixed grid to optimize spatial queries.
Written with Unity in mind (uses Vector2 and some Mathf functions). But is easily ported to support other environments as well.

## Usage

ContactGrid can be used to find out which objects making 'contact' with a certain shape. This is useful for determining if shapes intersect, or if for example objects are on screen.
The grid can also be used to quickly find the closest object to a given point. Although when the grid is very large and there are very little objects placed in the grid, this can be costly.

Copy the ContactGrid folder into your project.

```
var grid = new StaticGrid<SomeObjectType>(topLeft, width, height, cellSize);

grid.Add(someObject, new Point(x, y));
grid.Add(someObject, new Circle(x, y, r));
...

// Get all units that are overlapped by a circle
foreach (var someObject in grid.Contact(new Circle(x, y, radius))
    ...do something

// Get all units that a line intersects
foreach (var someObject in grid.Contact(new Line(x1, y1, x2, y2))
    ...do something

// Get all units that are on screen
foreach (var someObject in grid.Contact(new AABB(screenWorldTopLeft, screenWorldBottomRight))
    ...do something

// Get all units that a touch a point (this excludes other points or lines since they can't intersect a point)
foreach (var someObject in grid.Contact(new Point(x, y))
    ...do something

// Get the unit that is closest to a point
var nearestToPoint = grid.Nearest(new Vector2(x, y));

var dynamicGrid = new DynamicGrid<SomeKeyType, SomeObjectType>(topLeft, width, height, cellSize);

dynamicGrid.Add(someKey, someObject, new AABB(topLeft, bottomRight));
dynamicGrid.Add(someKey, someObject, new Line(x1, y1, x2, y2));

// Updates an objects bounding box
dynamicGrid.Update(someKey, new AABB(topLeft, bottomRight))

// Updates an objects point position
dynamicGrid.Update(someKey, new Point(x, y))

// Remove an object
dynamicGrid.Remove(someKey);

```

## When is it useful?

- When you have a lot of objects that are somewhat uniformly distributed within a fixed boundary.
- Your average search area is usually not much larger or smaller than the size of a single cell.
- You want to update positions/shapes of objects without much overhead.

## When not to use?

- When your map has very large dimensions or when you have a small number of objects.

## Benchmark

Idealized situation:

- Grid size: 10000x10000
- Cell size: 100x100
- Points: 100000
- Range search radius: 50
- Range queries: 10000
- Nearest queries: 10000

### Contact:

- List: 41844ms
- ContactGrid: 108ms (387x faster)

### Nearest:

- SearchGrid: 236ms (177x faster)
- List: 45821ms


## Components

### StaticGrid
Fastest performance but object positions/shapes are fixed once they are added.
Removal is slow.

### DynamicGrid
Uses an internal dictionary to keep track of units. This allows for updating of positions and fast removal.

### IConvexShape
You can define your custom shapes to look for in the grid if they implement IConvexShape.

## Future plans

- Right now contact grid can only be used to determine if certain objects touch each other and to retreive a unit that is closest to a point.
- I'm planning to incorporate methods that can also retreive the axes on which contact was made so that it can be used for collision handling.

- I'm also planning to add more shapes that can be added and queried with. Quads that can be rotated and polygons.

- Adjust some names/descriptions.

- There might still be some room for optimization.

Right now this is all I need for my own project(s).

by Bart van de Sande / Nonline
https://www.nonline.nl
