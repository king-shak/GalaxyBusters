using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailblazerMovement : MonoBehaviour
{
     // Private member variables.
    PlayerBehavior parent;
    private CooldownBarBehavior cooldownBar;

    private string[] movementAxes;
    private float speed;
    private string axis;
    private Vector3 startPos;
    private Vector3 endPos;
    private bool Blink;

    private float maxXPos = (20.0f / 3.0f) / 2.0f;
    private float maxYPos = 5.0f;

    private bool retrievedAxes;

    // Start is called before the first frame update
    void Start()
    {
        maxXPos -= (parent.transform.localScale.x / 2.0f);
        maxYPos -= (parent.transform.localScale.y / 2.0f);

        cooldownBar = parent.GetBasicAbilityCooldownBar();

        retrievedAxes = false;
    }

    // Update is called once per frame
    void Update()
    {
        RetrieveAxes();

        if (retrievedAxes)
        {
            UpdateSpeed();

            // Retrieve the values from our axes.
            float upDown = Input.GetAxis(movementAxes[0]);
            float leftRight = Input.GetAxis(movementAxes[1]);

            // Scale the values appropriately.
            upDown *= (speed * Time.deltaTime);
            leftRight *= (speed * Time.deltaTime);

            if (Input.GetAxis(axis) == 1.0f)
            {
                if (cooldownBar.ReadyToFire())
                {
                    // Blink
                    startPos = transform.position;
                    endPos = new Vector3(transform.position.x + (500 * leftRight), transform.position.y + (500 * upDown), transform.position.z);
                    endPos.x = Clip(-1.0f * maxXPos, maxXPos, endPos.x);
                    endPos.y = Clip(-1.0f * maxYPos, maxYPos, endPos.y);
                    Debug.Log(upDown);
                    if (startPos != endPos)
                    {
                        Blink = true;
                        cooldownBar.TriggerCooldown();
                        parent.GetComponent<Collider2D>().enabled = false;
                    }
                }
            }

            if (Blink)
            {
                transform.position = Vector3.Lerp(transform.position, endPos, 20f * Time.deltaTime);
                if (transform.position == endPos)
                {
                    Instantiate(Resources.Load("Prefabs/BulletDestructionZone"), transform.position, transform.rotation);
                    Blink = false;
                    parent.GetComponent<Collider2D>().enabled = true;
                }
            }
            else
            {
                Vector3 pos = transform.position;
                if ((upDown < 0.0f) && (pos.y <= (-1.0f * maxYPos + 0.4f)))
                {
                    upDown = 0.0f;
                }
                else if ((upDown > 0.0f) && (pos.y >= maxYPos))
                {
                    upDown = 0.0f;
                }

                if ((leftRight < 0.0f) && (pos.x <= (-1.0f * maxXPos)))
                {
                    leftRight = 0.0f;
                }
                else if ((leftRight > 0.0f) && (pos.x >= maxXPos))
                {
                    leftRight = 0.0f;
                }

                // Translate the player by the computed amount.
                transform.Translate(leftRight, upDown, 0.0f);
            }
        }
    }

    public void SetParent(PlayerBehavior parent)
    {
        this.parent = parent;
    }

    // Private helpers.
    private void RetrieveAxes()
    {
        if (!retrievedAxes)
        {
            movementAxes = parent.GetMovementAxes();
            bool verticalAxisValid = movementAxes[0] != null && movementAxes[0] != "";
            bool horizontalAxisValid = movementAxes[1] != null && movementAxes[1] != "";

            axis = parent.GetBasicAbilityAxis();
            bool axisValid = axis != null && axis != "";

            if (verticalAxisValid && horizontalAxisValid && axisValid)
            {
                retrievedAxes = true;
            }
        }
    }

    private void UpdateSpeed()
    {
        speed = parent.GetSpeed();
    }

    private float Clip(float min, float max, float val)
    {
        if (val < min)
        {
            return min;
        }
        else if (val > max)
        {
            return max;
        }
        else
        {
            return val;
        }
    }
}