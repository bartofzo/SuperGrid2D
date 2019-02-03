using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperGrid2D;
using System.Threading.Tasks;
using UnityEngine.UI;

public class Flocking : MonoBehaviour
{
    private const float LimitNearestDistance = 1f;

    public bool Flocks = false;

    public Slider AlignSlider;
    public Slider CohesionSlider;
    public Slider SeperationSlider;
    public Slider BoidsSlider;
    public Slider MaxVelSlider;
    public Button DefaultsButton;
    public Toggle FlockToggle;
    public Text BoidsText;

    public static float AroundRadius = 0.5f;
    public static float FieldOfViewDegrees = 135;

    public static float MaxAlignForce;
    public static float MaxCohesionForce;
    public static float MaxSeperationForce;
    public static float MaxVelocity;

    public static Vector2 TopLeft;
    public static Vector2 BottomRight;
    private static float smoothTimeValue;
    private static float width;
    private static float height;

    private static DynamicGrid2D<int, Particle> grid;
    private static List<Particle> updateUnits;

    public int UnitCount;
    public GameObject BoidPrefab;
    public GameObject MoleculePrefab;

    private static int BoidKeyCounter;

    // Start is called before the first frame update
    void Start()
    {
        TopLeft = Camera.main.ScreenToWorldPoint(Vector2.zero);
        BottomRight = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));
        width = BottomRight.x - TopLeft.x;
        height = BottomRight.y - TopLeft.y;

        resetDefaults();

        DefaultsButton.onClick.AddListener(resetDefaults);
        AlignSlider.onValueChanged.AddListener(onAlignChange);
        CohesionSlider.onValueChanged.AddListener(onCohesionChange);
        SeperationSlider.onValueChanged.AddListener(onSeperationChange);
        MaxVelSlider.onValueChanged.AddListener(onMaxVelChange);
        BoidsSlider.onValueChanged.AddListener(onBoidsChange);
        FlockToggle.onValueChanged.AddListener(onFlockToggle);
    }

    void CreateParticles()
    {
        if (updateUnits != null)
        {
            for (int i = updateUnits.Count - 1; i >= 0; i--)
            {
                Destroy(updateUnits[i].gameObject);
            }
        }

        grid = new DynamicGrid2D<int, Particle>(TopLeft, width, height, 4 * new Vector2(AroundRadius, AroundRadius));
        Debug.Log(grid.Width + "x" + grid.Height + " cellSize: " + grid.CellSize);
        updateUnits = new List<Particle>(UnitCount);

        if (Flocks)
        {
            for (int i = 0; i < UnitCount; i++)
            {
                Instantiate(BoidPrefab);
            }
        }
        else
        {
            for (int i = 0; i < UnitCount; i++)
            {
                Instantiate(MoleculePrefab);
            }
        }
    }

    void SetControls()
    {
        FlockToggle.isOn = Flocks;
        AlignSlider.enabled = Flocks;
        CohesionSlider.enabled = Flocks;
        SeperationSlider.enabled = Flocks;

        AlignSlider.minValue = 0;
        AlignSlider.maxValue = 0.1f;
        CohesionSlider.minValue = 0;
        CohesionSlider.maxValue = 0.05f;
        SeperationSlider.minValue = 0;
        SeperationSlider.maxValue = 0.05f;
        MaxVelSlider.minValue = 0.01f;
        MaxVelSlider.maxValue = 0.1f;

        BoidsSlider.minValue = 1;
        BoidsSlider.maxValue = 10000;
        BoidsSlider.wholeNumbers = true;
        BoidsText.text = "Units: " + UnitCount;

        AlignSlider.value = MaxAlignForce;
        CohesionSlider.value = MaxCohesionForce;
        SeperationSlider.value = MaxSeperationForce;
        MaxVelSlider.value = MaxVelocity;
        BoidsSlider.value = UnitCount;
    }

    void resetDefaults()
    {
        UnitCount = 100;

        Flocks = true;
      

        MaxAlignForce = 0.01f;
        MaxCohesionForce = 0.005f;
        MaxSeperationForce = 0.0025f;
        MaxVelocity = 0.05f;

        SetControls();
        CreateParticles();
    }

    void onAlignChange(float val)
    {
        MaxAlignForce = val;
    }

    void onCohesionChange(float val)
    {
        MaxCohesionForce = val;
    }

    void onSeperationChange(float val)
    {
        MaxSeperationForce = val;
    }

    void onMaxVelChange(float val)
    {
        MaxVelocity = val;
    }

    void onBoidsChange(float val)
    {
        int rounded = (int)val;
        BoidsText.text = "Boids: " + rounded;
        UnitCount = rounded;
        CreateParticles();
    }

    void onFlockToggle(bool val)
    {
        Flocks = val;
        SetControls();
        CreateParticles();
    }

    public static int RegisterParticle(Particle boid)
    {
        BoidKeyCounter++;
        grid.Add(BoidKeyCounter, boid, new Point(boid.Position));
        updateUnits.Add(boid);
        return BoidKeyCounter;
    }

    public static void UpdateGrid(Particle particle)
    {
        grid.Update(particle.Key,particle.Shape);
    }

    // Update is called once per frame
    void Update()
    {
        smoothTimeValue = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

        if (Time.renderedFrameCount % 10 == 0)
        {
            Debug.Log("Avg units per cell: " + grid.AverageUnitsPerCell + " avg cells searched: " + grid.AverageCellsSearched);
        }
    }

    private void FixedUpdate()
    {
        foreach (var boid in updateUnits)
        {
            boid.UpdateParticle();
            UpdateGrid(boid);
        }
    }

    public static Vector2 SmoothVector(Vector2 prev, Vector2 next)
    {
        return Vector2.Lerp(prev, next, smoothTimeValue);
    }

    public static float SmoothAngle(float prev, float next)
    {
        return Mathf.LerpAngle(prev, next, smoothTimeValue);
    }

    public static IEnumerable<Particle> InRadius(Boid boid, float radius)
    {
        return grid.ContactWhich(new Circle(boid.Position, radius), (other) =>
        {
            return other.Key != boid.Key;
        });
    }

    public static Particle NearestParticle(Particle particle)
    {
        return grid.GetNearestWhich(particle.Position, LimitNearestDistance, (other) => other.Key != particle.Key);
    }
}
