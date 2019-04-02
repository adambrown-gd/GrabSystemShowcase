using UnityEngine;
using System.Collections;
using System;

/*
Description: Utility class used for doing multiple math functions.
Creator: Alvaro Chavez Mixco
Creation Date:  Sunday, Novemeber 13, 2016
*/
namespace ACSL.Utility
{
    public class MathUtils
    {
        public const float M_DOUBLE_PI = 6.2831853f;
        public const float M_360_DEGREES = 360.0f;

        public static Vector3 Vector3Lerp(Vector3 a, Vector3 b, float t)
        {
            a.x = Mathf.Lerp(a.x, b.x, t);
            a.y = Mathf.Lerp(a.y, b.y, t);
            a.z = Mathf.Lerp(a.z, b.z, t);

            return a;
        }
        public static Vector3 Vector3LerpAngle(Vector3 a, Vector3 b, float t)
        {
            a.x = Mathf.LerpAngle(a.x, b.x, t);
            a.y = Mathf.LerpAngle(a.y, b.y, t);
            a.z = Mathf.LerpAngle(a.z, b.z, t);

            return a;
        }

        public static Quaternion LookAt(Vector3 dir, Vector3 up)
        {
            if (dir == Vector3.zero)
            {
                Debug.Log("Zero direction in MyLookRotation");
                return Quaternion.identity;
            }

            if (up != dir)
            {
                up.Normalize();
                Vector3 v = dir + up * -Vector3.Dot(up, dir);
                Quaternion q = Quaternion.FromToRotation(Vector3.forward, v);
                return Quaternion.FromToRotation(v, dir) * q;
            }
            else
            {
                return Quaternion.FromToRotation(Vector3.forward, dir);
            }
        }

        /*
        Description: Function to rotate a float to 2 decimal points precision, commonly used to prepare
        a float for UI Display
        Parameters: float aNumberToRound-The number to rotate
        Creator: Alvaro Chavez Mixco
        Creation Date:  Sunday, Novemeber 13, 2016
        */
        public static float RoundTo2Digits(float aNumberToRound)
        {
            return Mathf.Round(aNumberToRound * 100.0f) / 100.0f;//Round a float to only be 2 decimal points
        }

        /*
        Description: Helper function, somewhy I made it a function instead of an if statement, to 
        get the bool value of an int
        Parameters: int aIntValue-The value to convert
        Creator: Alvaro Chavez Mixco
        Creation Date:  Sunday, Novemeber 20, 2016
        */
        public static bool IntToBool(int aIntValue)
        {
            //If the int is positive
            if (aIntValue > 0)
            {
                return true;//Return true
            }
            else//If the int is 0, or a negative number
            {
                return false;//Return false
            }
        }

        /*
        Description: Helper function, somewhy I made it a function instead of an if statement, to 
        a bool to an int
        Parameters: int aBoolValue-The value to convert
        Creator: Alvaro Chavez Mixco
        Creation Date:  Sunday, Novemeber 20, 2016
        */
        public static int BoolToInt(bool aBoolValue)
        {
            //If the bool is true
            if (aBoolValue == true)
            {
                return 1;//Return 1
            }
            else//If the bool is false
            {
                return 0;//Return 0
            }
        }

        /*
        Description: Helper function to check if any of the values in a vector3
        is Not a Number
        Parameters: Vector3 aValueToCheck - The Vector3 that will be checked
        Creator: Alvaro Chavez Mixco
        */
        public static bool IsNaN(Vector3 aValueToCheck)
        {
            //If none of the values in the vector is Not a Number
            if (float.IsNaN(aValueToCheck.x) == false && float.IsNaN(aValueToCheck.y) == false
                && float.IsNaN(aValueToCheck.z) == false)
            {
                return false;
            }
            else//If any of the values in the vector is Not a Number
            {
                return true;
            }
        }

        /*
        Description: Helper function to get the total index of an element in a 2D Array
        Parameters: int aIndexX - The current X index of the element
                    int aIndexY - The current Y index of the element
                    int aArrayWidth - The total width of the 2D array
        Creator: Alvaro Chavez Mixco
        Creation Date:  Friday, January 27, 2017
        */
        public static int TotalIndex2DArray(int aIndexX, int aIndexY, int aArrayWidth)
        {
            return aIndexY * aArrayWidth + aIndexX;
        }

        /*
        Description: Helper function to get the index X of an element in a 2D array
        Parameters: int aTotalIndex - The total index of the element in the 2D array
                    int aArrayWidth - The total width of the 2D array
        Creator: Alvaro Chavez Mixco
        Creation Date:  Friday, January 27, 2017
        */
        public static int IndexX2DArray(int aTotalIndex, int aArrayWidth)
        {
            return aTotalIndex % aArrayWidth;
        }

        /*
        Description: Helper function to get the index Y of an element in a 2D array
        Parameters: int aTotalIndex - The total index of the element in the 2D array
                    int aArrayWidth - The total width of the 2D array
        Creator: Alvaro Chavez Mixco
        Creation Date:  Friday, January 27, 2017
        */
        public static int IndexY2DArray(int aTotalIndex, int aArrayWidth)
        {
            return aTotalIndex / aArrayWidth;
        }

        /*
        Description: Helper function to get the 3D (X,Y, Z) index of an object in a array according to its 1D intex
                     and the array size.
        Parameters: int a1DIndex - The total or 1D index being converted to 3D.
                    int aArrayWidth - The width, number fo columns, of the array
                    int aArrayHeight - The height of the array
        Creator: Alvaro Chavez Mixco
        Creation Date: Sunday, January 29, 2017
        */
        public static Vector3 Convert1DIndexTo3DArrayIndex(int a1DIndex, int aArrayWidth, int aArrayHeight)
        {
            Vector3 index3D;
            index3D.z = a1DIndex / (aArrayWidth * aArrayHeight);
            a1DIndex -= ((int)index3D.z * aArrayWidth * aArrayHeight);
            index3D.y = a1DIndex / aArrayWidth;
            index3D.x = a1DIndex % aArrayWidth;

            return index3D;
        }

        /*
        Description: Convert a 1D array of game objects into a 3D array of game objects. The function will 
                     return the new 3D array of game objects.
        Parameters: GameObject[] aArrayGameObject - The 1D array of game objects to convert
                    Vector3 aArrayXYZDimensions - The dimensions for the 3D array
        Creator: Alvaro Chavez Mixco
        Creation Date: Sunday, January 29, 2017
        */
        public static GameObject[,,] Convert1DArrayGameObjectsTo3DArray(GameObject[] aArrayGameObject, Vector3 aArrayXYZDimensions)
        {
            //Create the 3D array of game objects
            GameObject[,,] gameObjects3DArray = new GameObject[(int)aArrayXYZDimensions.x, (int)aArrayXYZDimensions.y, (int)aArrayXYZDimensions.z];

            Vector3 temp3DIndex;

            //If the 1D array matches in size with the desired 3D array
            if (aArrayXYZDimensions.x * aArrayXYZDimensions.y * aArrayXYZDimensions.z == aArrayGameObject.Length)
            {
                //Go through all the gameobjects
                for (int i = 0; i < aArrayGameObject.Length; i++)
                {
                    //Get the 3D coordinates according to the 1D invex
                    temp3DIndex = Convert1DIndexTo3DArrayIndex(i, (int)aArrayXYZDimensions.x, (int)aArrayXYZDimensions.y);

                    //Set the game object at the desired 3D intex
                    gameObjects3DArray[(int)temp3DIndex.x, (int)temp3DIndex.y, (int)temp3DIndex.z] = aArrayGameObject[i];
                }
            }

            return gameObjects3DArray;
        }

        /*
        Description: Get the value between elements in each axis.
        Parameters: Vector3 aTotalValues - The total value of the object
                    Vector3 aTotalNumberOfElements - The number of object that will divide the total value.
        Creator: Alvaro Chavez Mixco
        Creation Date: Sunday, January 29, 20177
        */
        public static Vector3 GetValuesBetweenElements(Vector3 aTotalValues, Vector3 aTotalNumberOfElements)
        {
            Vector3 valuesBetweenElements = Vector3.zero;

            //Get the value between each x,y and z element, according to the total value
            //and the total number of elements
            valuesBetweenElements.x = aTotalValues.x / aTotalNumberOfElements.x;
            valuesBetweenElements.y = aTotalValues.y / aTotalNumberOfElements.y;
            valuesBetweenElements.z = aTotalValues.z / aTotalNumberOfElements.z;

            return valuesBetweenElements;
        }

        /*
        Description: Check a 3 axis Euler rotation to ensure that it is within a 0 to 360 range.
                     The function will return a rotation between 0 to 360 in all of its axes.
        Parameters: Vector3 aDegreesRotation - The euler rotation to check
        Creator: Alvaro Chavez Mixco
        Creation Date: Sunday, January 29, 20177
        */
        public static Vector3 GetValidDegreesRotation(Vector3 aDegreesRotation)
        {

            Vector3 validRotation = aDegreesRotation;

            //Ensure no value is smaller than 0 in all degrees of rotation
            validRotation.x = GetValidDegreesRotation(validRotation.x);
            validRotation.y = GetValidDegreesRotation(validRotation.y);
            validRotation.z = GetValidDegreesRotation(validRotation.z);

            return validRotation;
        }

        /*
        Description: Check if a one axis of rotation is withing a 0 to 360 range.
        Parameters: float aDegreeRotation - The single rotation axis to check.
        Creator: Alvaro Chavez Mixco
        Creation Date: Sunday, January 29, 2017
        */
        public static float GetValidDegreesRotation(float aDegreeRotation)
        {
            float validRotation = aDegreeRotation;

            //While the rotation is less than 0
            while (aDegreeRotation < 0.0f)
            {
                //Add 360 to it
                aDegreeRotation += M_360_DEGREES;
            }

            //While the rotation is bigger than 360
            while (aDegreeRotation > M_360_DEGREES)
            {
                //Substract 360 from it
                aDegreeRotation -= M_360_DEGREES;
            }

            return validRotation;
        }

        /*
        Description: A function to calculate angle between current and previous forward direction and add it to comparison
        Parameters: aCurrentForwardDirection : Current forward direction
                    aPreviousForwardDirection : Previous forward direction
        Creator: Juan Calvin Raymond
        Creation Date: 22 Jan 2016
        */
        public static float CalculatingAngleDifference(Vector3 aCurrentForwardDirection, ref Vector3 aPreviousForwardDirection)
        {
            //If previous forward direction is not set yet
            if (aPreviousForwardDirection == Vector3.zero)
            {
                //Set previous forward direction to the current one
                aPreviousForwardDirection = aCurrentForwardDirection;
            }
            //If previous forward direction is already set
            else
            {
                //Return the angle between current and previous forward direction
                float angleDifference = Vector3.Angle(aCurrentForwardDirection, aPreviousForwardDirection);

                //Set previous forward direction to the current one
                aPreviousForwardDirection = aCurrentForwardDirection;

                return angleDifference;
            }

            //Return 0.0f if it haven't been calculated
            return 0.0f;
        }

        /*
        Description: A function to calculate angle between current and previous rotation and add it to comparison
        Parameters: aCurrentRotation : Current rotation
                    aPreviousRotation : Previous rotation
        Creator: Juan Calvin Raymond
        Creation Date: 22 Jan 2016
        */
        public static float CalculatingAngleDifference(float aCurrentRotation, ref float aPreviousRotation)
        {
            //If previous forward direction is not set yet
            if (aPreviousRotation == 0.0f)
            {
                //Set previous forward direction to the current one
                aPreviousRotation = aCurrentRotation;
            }
            //If previous forward direction is already set
            else
            {
                float angleDifference = aCurrentRotation - aPreviousRotation;

                //Set previous forward direction to the current one
                aPreviousRotation = aCurrentRotation;

                //Return the angle between current and previous forward direction
                return angleDifference;
            }

            //Return 0.0f if it haven't been calculated
            return 0.0f;
        }

        /*
        Description: A function to calculate angle between current and previous rotation and add it to comparison
        Parameters: out float aAngle - The angle of rotation according to axis
                    out Vector3 aAxis - The axis that will be checked 
                    Quaternion aRotation - The rotation where the axis and angle will be checked
        Creator: Alvaro Chavez Mixco
        Creation Date: Friday, March 24th, 2017
        */
        public static void RotationToAngleAxis360DegreesLimit(out float aAngle, out Vector3 aAxis, Quaternion aRotation)
        {
            //Get the amount of rotation in an axis
            aRotation.ToAngleAxis(out aAngle, out aAxis);

            //Check that the angle doesn't go beyond the "circle" 360 values
            if (aAngle > 180)
            {
                aAngle -= 360;
            }
        }
        /*
        Description: A function to clamp an angle with a custom limit
        Parameters: float angle - the angle to clamp
                    float min - the minimum angle
                    float max - the maximum angle
        Creator: Adam Brown
        Creation Date:  November 25th, 2018
        */
        public static float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f)
                angle = 360 + angle;
            if (angle > 180f)
                return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }

        /*
        Description: A function to clamp all the values of a vector 3 to a 0 to 1 range
        Parameters: Vector3 aVector3ToClamp - The vector 3 to clamp
                    Vector3 aMinValues - The min values of the vector3 
                    Vector3 aMaxValues - The max values of the vector3
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, April 10th, 2017
        */
        public static Vector3 Vector3Clamp(Vector3 aVector3ToClamp, Vector3 aMinValues, Vector3 aMaxValues)
        {
            aVector3ToClamp.x = Mathf.Clamp(aVector3ToClamp.x, aMinValues.x, aMaxValues.x);
            aVector3ToClamp.y = Mathf.Clamp(aVector3ToClamp.y, aMinValues.y, aMaxValues.y);
            aVector3ToClamp.z = Mathf.Clamp(aVector3ToClamp.z, aMinValues.z, aMaxValues.z);

            return aVector3ToClamp;
        }

        /*
        Description: A function to clamp all the values of a vector 3 to a 0 to 1 range
        Parameters: Vector3 aVector3ToClamp - The vector 3 to clamp
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, April 10th, 2017
        */
        public static Vector3 Vector3Clamp01(Vector3 aVector3ToClamp)
        {
            aVector3ToClamp.x = Mathf.Clamp01(aVector3ToClamp.x);
            aVector3ToClamp.y = Mathf.Clamp01(aVector3ToClamp.y);
            aVector3ToClamp.z = Mathf.Clamp01(aVector3ToClamp.z);

            return aVector3ToClamp;
        }

        /*
        Description: A function to clamp all the values of a vector 3 to a min and max value
        Parameters: Vector3 aVector3ToClamp - The vector 3 to clamp
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, April 10th, 2017
        */
        public static Vector3 Vector3Clamp(Vector3 aVector3ToClamp, float aMinValue, float aMaxValue)
        {
            aVector3ToClamp.x = Mathf.Clamp(aVector3ToClamp.x, aMinValue, aMaxValue);
            aVector3ToClamp.y = Mathf.Clamp(aVector3ToClamp.y, aMinValue, aMaxValue);
            aVector3ToClamp.z = Mathf.Clamp(aVector3ToClamp.z, aMinValue, aMaxValue);

            return aVector3ToClamp;
        }

        /*
        Description: A function to return random values between a certain range
        Parameters: Vector3 aMinRange, 
                    Vector3 aMaxRange
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, April 10th, 2017
        */
        public static Vector3 RandomVector3(Vector3 aMinRange, Vector3 aMaxRange)
        {
            Vector3 randomVector = Vector3.zero;

            randomVector.x = UnityEngine.Random.Range(aMinRange.x, aMaxRange.x);
            randomVector.y = UnityEngine.Random.Range(aMinRange.y, aMaxRange.y);
            randomVector.z = UnityEngine.Random.Range(aMinRange.z, aMaxRange.z);

            return randomVector;
        }

        public static Vector3 RandomVector4(Vector4 aMinRange, Vector4 aMaxRange)
        {
            Vector4 randomVector = Vector4.zero;

            randomVector.x = UnityEngine.Random.Range(aMinRange.x, aMaxRange.x);
            randomVector.y = UnityEngine.Random.Range(aMinRange.y, aMaxRange.y);
            randomVector.z = UnityEngine.Random.Range(aMinRange.z, aMaxRange.z);
            randomVector.w = UnityEngine.Random.Range(aMinRange.w, aMaxRange.w);

            return randomVector;
        }

        /*
        Description: A function to check if the values of a vector are almost equals to the value of 
                    another vector
        Parameters: Vector3 aVectorA
                    Vector3 aVectorB
        Creator: Alvaro Chavez Mixco
        Creation Date: Wednesday,  April 26th, 2017
        */
        public static bool AlmostEqualVector3(Vector3 aVectorA, Vector3 aVectorB)
        {
            return Mathf.Approximately(aVectorA.x, aVectorB.x)
                && Mathf.Approximately(aVectorA.y, aVectorB.y)
                && Mathf.Approximately(aVectorA.z, aVectorA.z);
        }

        /*
        Description: A function to lerp a value "back" to half, according to if its value is above or below half.
        Parameters: float aPercentToLerp - The value that will be lerped 
                    float aLerpSpeed - The speed at which the value will be lerped
                    float aBias - The max variation the lerped percent can be from half, this is used to avoid jitteriness when values
                                 constantly switch between above and below halfs
        Creator: Alvaro Chavez Mixco
        Creation Date: Sunday,  April 30th, 2017
        */
        public static float LevelPercentToHalf(float aPercentToLerp, float aLerpSpeed = 1.0f, float aBias = 0.01f)
        {
            //Ensure the value is a 0 to 1 percent
            float lerpedPercent = Mathf.Clamp01(aPercentToLerp);

            //If value is bigger than half
            if (lerpedPercent > 0.5f + aBias)
            {
                //Reduce it
                lerpedPercent -= Time.deltaTime * aLerpSpeed;
            }
            else if (lerpedPercent < 0.5f - aBias)//If percent is less than half
            {
                //Increase it
                lerpedPercent += Time.deltaTime * aLerpSpeed;
            }

            //Ensure the returned value is within a 0 to 1 range
            return Mathf.Clamp01(lerpedPercent);
        }

        public static float PositiveOrNegativeCondition(bool aCondition)
        {
            if (aCondition == true)
            {
                return 1.0f;
            }
            else
            {
                return -1.0f;
            }
        }

        public static float RescaleRange(float num, float low1, float high1, float low2, float high2)
        {
            return low2 + (num - low1) * (high2 - low2) / (high1 - low1);
        }

        public static float RescaleRangeClamp(float num, float low1, float high1, float low2, float high2)
        {
            return Mathf.Clamp01(low2 + (num - low1) * (high2 - low2) / (high1 - low1));
        }

        public static bool AlmostEquals(float aValueA, float aValueB, float aThreshold = 0.01f)
        {
            float differences = aValueB - aValueA;
            differences = differences < 0.0f ? -differences : differences;
            return differences < aThreshold;
        }

        //Closest point to a sphere radius, this can't be only in the radius of the sphere and not inside it
        public static Vector3 GetClosestPointToSphereRadius(Vector3 aPositionBeingChecked, Vector3 aPositionCenterOfSphere, float aSphereRadius)
        {
            //Get the normalized direction from the spere
            Vector3 directionFromphere = Vector3.Normalize(aPositionBeingChecked - aPositionCenterOfSphere);

            //Expand the direction to be on the edge of the radius
            directionFromphere *= aSphereRadius;

            //Offset the direction found, by the world center of the sphere
            return directionFromphere + aPositionCenterOfSphere;
        }

        //If the position is already inside the sphere it will just return the position it already has
        public static Vector3 GetClosestPointToSphere(Vector3 aPositionBeingChecked, Vector3 aPositionCenterOfSphere, float aSphereRadius)
        {
            if (aSphereRadius * aSphereRadius < Vector3.SqrMagnitude(aPositionBeingChecked - aPositionCenterOfSphere))
            {
                return GetClosestPointToSphereRadius(aPositionBeingChecked, aPositionCenterOfSphere, aSphereRadius);
            }
            else
            {
                return aPositionBeingChecked;
            }
        }

        /*
        Description: Get the distance between two game objects, only in the X and Z, ignoring the Y.
        Parameters: Vector3 aGameObjectPos - One object being used to measure distance 
                    Vector3 aAnotherGameObjectPos - The other object being used to measure distance 
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static float GetDistanceXZ(Vector3 aGameObjectPos, Vector3 aAnotherGameObjectPos)
        {

            //Return the distance of the object ignoring the Y
            return Vector2.Distance(new Vector2(aGameObjectPos.x, aGameObjectPos.z),
               new Vector2(aAnotherGameObjectPos.x, aAnotherGameObjectPos.z));
        }


        /*
        Description: Get the square distance between two game objects
        Parameters: Vector3 aGameObjectPos - One object being used to measure distance 
                    Vector3 aAnotherGameObjectPos - The other object being used to measure distance 
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static float GetDistanceSquared(Vector3 aGameObjectPos, Vector3 aAnotherGameObjectPos)
        {

            //Return the square distance of the object to the camera
            return Vector3.SqrMagnitude(aGameObjectPos -
                aAnotherGameObjectPos);
        }

        /*
        Description: Get the squared distance between two game objects, only in the X and Z, ignoring the Y.
        Parameters: Vector3 aGameObjectPos - One object being used to measure distance 
                    Vector3 aAnotherGameObjectPos - The other object being used to measure distance 
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static float GetDistanceXZSquared(Vector3 aGameObjectPos, Vector3 aAnotherGameObjectPos)
        {
            //Return the distance of the object to the camera ignoring the Y
            return Vector2.SqrMagnitude(new Vector2(aGameObjectPos.x, aGameObjectPos.z) -
               new Vector2(aAnotherGameObjectPos.x, aAnotherGameObjectPos.z));
        }

        /*
        Description: Get the direction from the targeting position toward the target position
        Parameters: Vector3 aTargetPosition - The position that will be aimed towards
                    Vector3 aPositionTargeting - The position where it would be aiming from
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static Vector3 GetDirectionTowardTarget(Vector3 aTargetPosition, Vector3 aPositionTargeting)
        {
            return (aTargetPosition - aPositionTargeting).normalized;
        }

        /*
        Description: Get the direction from the targeting position toward the target position, ignoring the Y value,
                     so only X and Z will be considered
        Parameters: Vector3 aTargetPosition - The position that will be aimed towards
                    Vector3 aPositionTargeting - The position where it would be aiming from
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static Vector3 GetDirectionTowardTarget2DXZ(Vector3 aTargetPosition, Vector3 aPositionTargeting)
        {
            //Cancel the Y position
            aTargetPosition.y = 0.0f;
            aPositionTargeting.y = 0.0f;

            return GetDirectionTowardTarget(aTargetPosition, aPositionTargeting);
        }

        /*
        Description: Returns the XZ positions evenly spaced along the circumference of a circle 
                     at origin 0,0
        Parameters: int aNumObjectsToOffset - The number of objects that will be placed along the circumference
                    float aCircleRadius - The radius of the circle
                    bool aPlaceFirstObjectInCenter - If true the first position will be at 0,0 or center of the circle. If false    
                                                     the first position will be placed along the circumference and used to evenly
                                                     distrubute the other positions
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static Vector2[] CalculateXZPositionsAlongCircumference(int aNumObjectsToOffset,
            float aCircleRadius, bool aPlaceFirstObjectInCenter = false)
        {
            //Create an array of the players to offset
            Vector2[] positions = new Vector2[aNumObjectsToOffset];

            //If there is at least one object to offset
            if (aNumObjectsToOffset > 0)
            {
                int startingIndex = 0;

                //If the first object will be on the center of the circle
                if (aPlaceFirstObjectInCenter == true)
                {
                    //Set no offset for him
                    positions[0] = Vector2.zero;

                    //Increase the starting index to mark that the first object already has an offset
                    startingIndex = 1;
                }

                //Get the number of objects that will be placed around in a circle
                int objectsToPlaceAround = aNumObjectsToOffset - startingIndex;

                //Calculate the angle between all the players so that they are evenly spaced between each other (along the circle circumference)
                //This is done in radians to simplify the math, since Unity Sin and Cos function use radians
                float angleBetweenObjects = M_DOUBLE_PI / objectsToPlaceAround;

                //Go through each offset that will be calculated
                for (int i = startingIndex; i < aNumObjectsToOffset; i++)
                {
                    //Calculate the postion of the offset along the circle circumference
                    positions[i] = new Vector2(
                        Mathf.Cos(i * angleBetweenObjects) * aCircleRadius,
                        Mathf.Sin(i * angleBetweenObjects) * aCircleRadius);
                }
            }

            return positions;
        }

        /*
        Description: Returns the XZ position along the circumference of an object at a certain index. This is an optimization of,
                    CalculateXZPositionsAlongCircumference. Used to obtain only a single position instead of all of them.
        Parameters: int aIndexToCalculate - The index of the position that will be obtained
                    int aNumObjectsToOffset - The number of objects that will be placed along the circumference
                    float aCircleRadius - The radius of the circle
                    bool aPlaceFirstObjectInCenter - If true the first position will be at 0,0 or center of the circle. If false    
                                                     the first position will be placed along the circumference and used to evenly
                                                     distribute the other positions
        Creator: Alvaro Chavez Mixco
        Creation Date: Monday, October 9th, 2017
        */
        public static Vector2 CalculateXZPositionAtIndexAlongCircumference(int aIndexToCalculate, int aNumObjectsToOffset,
            float aCircleRadius, bool aPlaceFirstObjectInCenter = false)
        {
            //If there is at least one object to offset
            if (aNumObjectsToOffset > 0 && aIndexToCalculate < aNumObjectsToOffset && aIndexToCalculate >= 0)
            {
                int startingIndex = 0;

                //If the first object will be on the center of the circle
                if (aPlaceFirstObjectInCenter == true)
                {
                    //If the index to obtain is 0
                    if (aIndexToCalculate == 0)
                    {
                        //Return that there is no offset
                        return Vector2.zero;
                    }

                    //Increase the starting index to mark that the first player already has an offset
                    startingIndex = 1;
                }

                //Get the number of players that will be placed around in a circle
                int objectsToPlaceAround = aNumObjectsToOffset - startingIndex;

                //Calculate the angle between all the players so that they are evenly spaced between each other (along the circle circumference)
                //This is done in radians to simplify the math, since Unity Sin and Cos function use radians
                float angleBetweenObjects = M_DOUBLE_PI / objectsToPlaceAround;

                //Calculate the postion of the offset along the circle circumference
                return new Vector2(
                    Mathf.Cos(aIndexToCalculate * angleBetweenObjects) * aCircleRadius,
                    Mathf.Sin(aIndexToCalculate * angleBetweenObjects) * aCircleRadius);
            }

            return Vector2.zero;
        }
    }

}