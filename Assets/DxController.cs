using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DxController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float handleSpeed;
    [SerializeField] private float ballSpeed;


    private Material material;

    private float _handlePosition = .35f;

    private Vector2 _ballDirection = new Vector2(1, -1f);
    private Texture2D _texture2D;

    private List<BoxData> _boxDataContainer = new List<BoxData>();
    private int _boxPixelSize = 25;

    void Start()
    {
        material = meshRenderer.material;
        material.SetFloat("_HandleX", _handlePosition);
        Reset();
    }

    void Update()
    {
        MoveHandle();
        MoveBall();
    }

    private void MoveHandle()
    {
        var horizontal = Input.GetAxis("Horizontal");
        if (horizontal == 0) return;
        
        var minPos = 0f;
        var maxPos = .73f;

        _handlePosition += horizontal * handleSpeed * Time.deltaTime;
        _handlePosition = Mathf.Clamp(_handlePosition, minPos, maxPos);
        material.SetFloat("_HandleX", _handlePosition);
    }

    private void MoveBall()
    {
        var posColor = material.GetColor("_BallPos");
        var pos = new Vector2(posColor.r, posColor.g);
        pos += _ballDirection * ballSpeed * Time.deltaTime;

        var failed = CheckFailed(pos);
        if (failed) return;

        var directionChanged = CheckWalls(pos);
        directionChanged = CheckHandle(pos);
        directionChanged = CheckBoxCollision(pos);

        if (directionChanged)
        {
            posColor = material.GetColor("_BallPos");
            pos = new Vector2(posColor.r, posColor.g);
            pos += _ballDirection * ballSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, 0, 1);
        pos.y = Mathf.Clamp(pos.y, 0, 1);
        material.SetColor("_BallPos", new Color(pos.x, pos.y, 0, 0));

    }


    private bool CheckWalls(Vector2 pos)
    {
        if (pos.x is < 0 or > 1)
        {
            _ballDirection.x *= -1;
            return true;
        }

        if (pos.y > 1)
        {
            _ballDirection.y *= -1;
            return true;
        }

        return false;
    }

    private bool CheckHandle(Vector2 pos)
    {
        var handleX = material.GetFloat("_HandleX");
        var handleY = material.GetFloat("_HandleY");
        var handleWidth = material.GetFloat("_HandleWidth");
        var handleHeight = material.GetFloat("_HandleHeight");

        var upperBorder = handleY + handleHeight;
        var leftBorder = handleX;
        var rightBorder = handleX + handleWidth;

        if (pos.y > upperBorder) return false;
        if (pos.x < leftBorder || pos.x > rightBorder) return false;


        _ballDirection.y *= -1;
        
        return true;
    }

    private bool CheckBoxCollision(Vector2 pos)
    {
        var activeBoxes = _boxDataContainer.Where(b => b.Active);
        var closestBox = activeBoxes.OrderBy(b => Vector3.Distance(pos, new Vector2((float)b.X/ _boxPixelSize, (float)b.Y/ _boxPixelSize))).FirstOrDefault();
        if (closestBox == null) return false;

        var boxWith = 1 / (float)_boxPixelSize;

        var distance = Vector2.Distance(pos, new Vector2((float)closestBox.X / _boxPixelSize, (float)closestBox.Y / _boxPixelSize));
        if (distance > boxWith) return false;


        closestBox.Active = false;
        UpdatePixel(closestBox.X, closestBox.Y, false);

        // find ball's relative position to box and change direction accordingly
        var relativePos = pos - new Vector2(closestBox.X, closestBox.Y);
        var x = relativePos.x;
        var y = relativePos.y;

        if (x > y)
        {
            _ballDirection.x *= -1;
            return true;
        }

        _ballDirection.y *= -1;
        return true;
    }

    private bool CheckFailed(Vector2 pos)
    {
        var handleY = material.GetFloat("_HandleY");

        if (pos.y < handleY)
        {
            material.SetColor("_BallPos", new Color(.5f, .5f, 0, 0));
            _ballDirection.y *= -1;
            Reset();
            return true;
        }

        return false;
    }


    private void UpdatePixel(int xPos, int yPos, bool active)
    {
        var color = _texture2D.GetPixel(xPos, yPos);
        color.a = active ? 1 : 0;
        _texture2D.SetPixel(xPos, yPos, color);
        _texture2D.Apply();
    }

    private void GenerateLookupTexture()
    {
        int width = _boxPixelSize;
        int height = _boxPixelSize;
        var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        for (int y = 0; y < tex.height; ++y)
        {
            for (int x = 0; x < tex.width; ++x)
            {
                Color c = new Color(0, 0, 0, 0);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        byte[] bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(Application.dataPath + "/BitwiseLUT.png", bytes);

        _texture2D = tex;
        material.SetTexture("_MainTex", _texture2D);
    }


    private class BoxData
    {
        public int X;
        public int Y;
        public bool Active;
    }

    private void GenerateBoxes()
    {
        _boxDataContainer.Clear();
        var horizontalValue = 1;
        var maxVerticalValue = 7;

        while (horizontalValue < 25)
        {
            for (int i = 2; i < maxVerticalValue; i += 2)
            {
                var boxData = new BoxData();
                boxData.X = horizontalValue;
                boxData.Y = 25 - i;
                UpdatePixel(horizontalValue, boxData.Y, true);
                boxData.Active = true;
                _boxDataContainer.Add(boxData);
            }

            horizontalValue += 2;
        }
    }

    private void Reset()
    {
        GenerateLookupTexture();
        GenerateBoxes();
        
    }
}