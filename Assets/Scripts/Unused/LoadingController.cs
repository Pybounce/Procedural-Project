using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    //Basic plan:
    /*
     * Give it a max value (the amount of tasks to complete until loaded
     * Then increment it's currentLoad value by 1 for each task
     * It displays the loading bar at the correct percentage
     * It will automatically remove the canvas screen once it's at 100%
     * 
     * It has a list of gameObjects that must be turned on after load is complete
     */
    private int totalTasks;                        //Total amount of tasks it takes to fully load the scene
    private int tasksComplete;                     //Current number of tasks complete
    [SerializeField] private List<GameObject> postLoadObjects;  //Objects that should be active only after loading is done

    [SerializeField] Canvas canvas;
    [SerializeField] Slider slider;

    public void TaskDone()
    {
        tasksComplete += 1;
        float progress = (float)tasksComplete / (float)totalTasks;
        slider.value = progress;
        
    }

    public void SetTotalTasks(int n)
    {
        totalTasks = n;
    }

    public bool IsLoaded()
    {
        if (tasksComplete >= totalTasks)
        {
            canvas.enabled = false;
            foreach (GameObject o in postLoadObjects)
            {
                o.SetActive(true);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

}
