using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseGenerator : MonoBehaviour
{
    public Texture2D tex;
    public float maxDist, pulseSpeed;
    public Material pulseMat;
    public int x, y, res;
    public Transform leftFoot, rightFoot;
    public bool right;

    private void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                tex.SetPixel(i, j, Color.white);
            }
        }
        tex.Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StopAllCoroutines();
            StartCoroutine(PulseWave());
        }
    }

    IEnumerator PulseWave()
    {
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 0), new Vector2(0, 0)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 10), new Vector2(0, 1)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 20), new Vector2(0, 2)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 30), new Vector2(0, 3)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 40), new Vector2(0, 4)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 50), new Vector2(0, 5)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 60), new Vector2(0, 6)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 70), new Vector2(0, 7)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 80), new Vector2(1, 0)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 90), new Vector2(1, 1)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 100), new Vector2(1, 2)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 110), new Vector2(1, 3)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 120), new Vector2(1, 4)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 130), new Vector2(1, 5)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 140), new Vector2(1, 6)));
        yield return new WaitForSeconds(.7f);
        StartCoroutine(GeneratePulse(new Vector3(0, 4, 150), new Vector2(1, 7)));
    }

    public void FootStep()
    {
        if(right)
            StartCoroutine(GeneratePulse(rightFoot.position, new Vector2(x, y)));
        else
            StartCoroutine(GeneratePulse(leftFoot.position, new Vector2(x, y)));

        right = !right;

        x = x + 1 < res ? x + 1 : 0;
        y = x == 0 ? y + 1 : y;
        y = y == res ? 0 : y;
    }

    IEnumerator GeneratePulse(Vector3 worldPos, Vector2 id)
    {
        Color pos = new Color(worldPos.x /maxDist, worldPos.y / maxDist, worldPos.z / maxDist, 0);
        tex.SetPixel((int)id.x, (int)id.y, pos);
        Debug.Log("Color Set To X: " + pos.r * maxDist + " Y: " + pos.g * maxDist + " Z: " + pos.b * maxDist + " D: " + pos.a * maxDist);
        tex.Apply();

        pulseMat.SetTexture("_DataTexture", tex);

        pulseMat.SetVector("_PulseOrigin", worldPos);
        float time = 10;
        float speedCoeff = 1;
        float pulseDist = 0;
        while (time > 0)
        {
            pulseDist += Time.deltaTime * pulseSpeed * speedCoeff;
            time -= Time.deltaTime;
            speedCoeff += Time.deltaTime * 3;
            pos = new Color(pos.r, pos.g, pos.b, pulseDist / maxDist);
            tex.SetPixel((int)id.x, (int)id.y, pos);
            tex.Apply();
            Debug.Log("Color Set To X: " + pos.r * maxDist + " Y: " + pos.g * maxDist + " Z: " + pos.b * maxDist + " D: " + pos.a * maxDist);
            pulseMat.SetFloat("_PulseDistance", pulseDist);
            yield return null;
        }
        pulseMat.SetFloat("_PulseDistance", 0);
    }
}
