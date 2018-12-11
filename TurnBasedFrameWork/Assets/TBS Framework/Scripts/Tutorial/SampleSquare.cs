using UnityEngine;

class SampleSquare : Square
{
    Color _originalColor = Color.white;
    private void Awake()
    {
        _originalColor = GetComponent<Renderer>().material.color;
    }

    public override Vector3 GetCellDimensions()
    {
        return GetComponent<Renderer>().bounds.size;
    }

    public override void MarkAsHighlighted()
    {
        GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);
    }

    public override void MarkAsPath()
    {
        GetComponent<Renderer>().material.color = Color.green;
    }

    public override void MarkAsReachable()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void MarkAsSpawnableCell()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public override void UnMark()
    {
        GetComponent<Renderer>().material.color = _originalColor;

    }
}

