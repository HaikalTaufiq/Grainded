using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishInfo", menuName = "Fishing Collection/New Fish Info")]

public class FishInfo : ScriptableObject
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _type;
    [SerializeField] private Sprite _image;
    [SerializeField] private string _name;
    [SerializeField] private string _desc;
    [SerializeField] private float _length;
    [SerializeField] private float _Maxweight;

    public GameObject prefab => this._prefab;
    public Sprite image => this._image;
    public string type => this._type;
    public string Name => this._name;
    public string description => this._desc;
    public float length => this._length;
    public float Maxweight => this._Maxweight;

}
