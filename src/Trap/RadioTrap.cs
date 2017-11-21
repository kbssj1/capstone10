using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioTrap : MonoBehaviour {

    private Vector3 endPosition = new Vector3(-6.84f, -17.6f, -8.261f);
    public Transform _target;

    public float speed = 1.0f;
    private float startTime;
    private float journeyLength;

    void OnEnable()
    {

        startTime = Time.time; // 시간
        journeyLength = Vector3.Distance(_target.position, endPosition);  // 이동 거리
        StartCoroutine(move());
    }


    public IEnumerator move()
    {
        yield return null;
        while (true)
        {

            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            _target.position = Vector3.Lerp(_target.position, endPosition, fracJourney); // 시작위치에서 끝위치까지 이동
            _target.rotation = new Quaternion(0f, 180f, 0f, 0f);
            if (_target.position == endPosition)
            {
                this.enabled = false;
                break;
            }
            yield return null;
        }
    }
}
