using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform objetivo;
    public float altura = 10f;
    public float distancia = 10f;

    public float suavizadoMax = 5f;
    public float suavizadoMin = 5f;

    private Vector3 offset; // Distancia inicial entre la cámara y el objetivo
    public float currentDistance;
    void Start()
    { 
        offset = new Vector3(0f, altura, -distancia);
        Vector3 posicionDeseada = objetivo.position + offset;
        Vector3 posicionSuavizada = posicionDeseada;
        transform.position = posicionSuavizada;
        transform.LookAt(objetivo.position);
    }

    void FixedUpdate()
    {
        offset = new Vector3(0f, altura, -distancia);
        Vector3 posicionDeseada = objetivo.position + offset;
        currentDistance =  Vector3.Distance(transform.position, posicionDeseada) ;

        float suavizado = Mathf.Lerp(suavizadoMin, suavizadoMax,  currentDistance );
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
        transform.position = posicionSuavizada;    

    }
}
