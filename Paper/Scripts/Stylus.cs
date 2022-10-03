using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Stylus : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 15;
    public InputActionProperty pinchAnimationAction;
    // public Controller controller;

    private Renderer _renderer;
    private Renderer _paperRenderer;
    private Color[] _colors;
    private Color[] _colorsOriginal;
    private float _tipHeight;
    private RaycastHit _touch;
    private Paper _paper;
    private Vector2 _position, _lastPosition;
    private bool _lastFrame;
    private Quaternion _lastRotation;


    // Start is called before the first frame update
    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = _tip.localScale.y;

    }

    // Update is called once per frame
    void Update()
    {
        Draw();
    }
    private void Draw(){
      if(Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
      {
        if(_touch.transform.CompareTag("Paper"))
        {
          if(_paper == null)
          {
            Debug.Log("paper is null");
            _paper = _touch.transform.GetComponent<Paper>();
            _colorsOriginal = _paper.texture.GetPixels(0);
          }
          _position = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);
          var x = (int)(_position.x * _paper.textureSize.x - (_penSize/2));
          var y = (int)(_position.y * _paper.textureSize.y - (_penSize/2));

          if(isOnPaper(x, y)) return;
          if(_lastFrame)
          {
            if (OVRInput.Get(OVRInput.Button.One)) {
              drawLine(x, y);
            } else if (OVRInput.Get(OVRInput.Button.Two)) {
              eraseLine(x, y);
            }
            transform.rotation = _lastRotation;
            _paper.texture.Apply();
          }
          _lastPosition = new Vector2(x, y);
          _lastRotation = transform.rotation;
          _lastFrame = true;
          return;
        }
        Debug.Log("not paper");
      }
      Debug.Log("not raycast");
      _paper = null;
      _lastFrame = false;
    }

    private bool isOnPaper(int x, int y) 
    {
      return y < 0 || y > _paper.textureSize.y || x < 0 || x > _paper.textureSize.x;
    }

    private void drawLine(int x, int y)
    {
      _paper.texture.SetPixels(x, y, _penSize, _penSize, _colors);
      for (float f=0.01f; f<1.00f; f+=0.01f) {
        var lerpX = (int)Mathf.Lerp(_lastPosition.x, x,f);
        var lerpY = (int)Mathf.Lerp(_lastPosition.y, y,f);
        _paper.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);
      } 
    }

    private void eraseLine(int x, int y)
    {
      _paper.texture.SetPixels(x, y, _penSize, _penSize, _colorsOriginal);
      for(float f=0.01f; f<1.00f; f+=0.01f) {
        var lerpX = (int)Mathf.Lerp(_lastPosition.x, x,f);
        var lerpY = (int)Mathf.Lerp(_lastPosition.y, y,f);
        _paper.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colorsOriginal);
      }
    }
}

// Reference: https://www.youtube.com/watch?v=sHE5ubsP-E8
