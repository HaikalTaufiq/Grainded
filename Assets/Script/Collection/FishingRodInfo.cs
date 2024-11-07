using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishingRodInfo", menuName = "Fishing Collection/New Fishing Rod Info")]
public class FishingRodInfo : ScriptableObject
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _type;
    [SerializeField] private Sprite _image;
    [SerializeField] private string _name;
    [SerializeField] private string _desc;
    [SerializeField] private float _length;
    [SerializeField] private float _maxWeight;

    public GameObject prefab => this._prefab;
    public Sprite image => this._image;
    public string type => this._type;
    public string Name => this._name;
    public string description => this._desc;
    public float length => this._length;
    public float maxWeight => this._maxWeight;


}
