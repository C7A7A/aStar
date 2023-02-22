using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathMarker {
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p) {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj) {
        if (obj == null) return false;
        if (!(obj is PathMarker)) return false;

        return location.Equals(((PathMarker)obj).location);
    }

    public override int GetHashCode() {
        return 0;
    }
}

public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;
    bool done = false;

    void RemoveAllMarkers() {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers) {
            Destroy(m);
        }
    }

    void BeginSearch() {
        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();
        for (int z = 1; z < maze.depth - 1; z++) {
            for (int x = 1; x < maze.width - 1; x++) {
                if (maze.map[x,z] != 1) {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }

        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0, Instantiate(end, goalLocation, Quaternion.identity), null);
    
        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode) {
        if (thisNode == null) {
            return;
        }

        if (thisNode.Equals(goalNode)) {
            done = true;
            return;
        }

        foreach (MapLocation dir in maze.directions) {
            MapLocation neighbour = dir + thisNode.location;

            // check if neighbour is a wall
            if (maze.map[neighbour.x, neighbour.z] == 1) {
                continue;
            }
            
            // check if neighbour is outside of the maze
            if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.z < 1 || neighbour.z >= maze.depth) {
                continue;
            }

            if (IsClosed(neighbour)) {
                continue;
            }

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float F = G + H;

            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0, neighbour.z * maze.scale), Quaternion.identity);

            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[1].text = "H: " + H.ToString("0.00");
            values[2].text = "F: " + F.ToString("0.00");

            if (!UpdateMarker(neighbour, G, H, F, thisNode)) {
                open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
            }
        }

        open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();
        PathMarker pm = (PathMarker) open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt) {
        foreach (PathMarker p in open) {
            if (p.location.Equals(pos)) {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker) {
        foreach (PathMarker p in closed) {
            if (p.location.Equals(marker)) return true;
        }

        return false;
    }

    void Start()
    {
        
    }

    void GetPath() {
        RemoveAllMarkers();
        PathMarker thisMarker = lastPos;

        while (thisMarker != null) {
            MapLocation pathMarkerLocation = thisMarker.location;
            Instantiate(pathP, new Vector3(pathMarkerLocation.x * maze.scale, 0, pathMarkerLocation.z * maze.scale), Quaternion.identity);

            thisMarker = thisMarker.parent;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            BeginSearch();
        }

        if (Input.GetKeyDown(KeyCode.C) && !done) {
            // print("Search");
            Search(lastPos);
        }
        
        if (Input.GetKeyDown(KeyCode.M) && done) {
            // print("GetPath");
            GetPath();
        }
    }
}
