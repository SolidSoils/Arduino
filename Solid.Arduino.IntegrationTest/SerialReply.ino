/*
   This example is intended to demonstrate the SolidSoils4Arduino library's
   SerialConnection.FindSerialConnection(string query, string expectedReply) method.
   
   Created February 11th, 2015
   by Henk van Boeijen. (info@solidsoils.nl)
 
   See https://github.com/SolidSoils/Arduino
   
   This example code is in the public domain.
 */

#include <Firmata.h>

char query[] = "Hello?";
char reply[] = "Arduino!";

void setup()
{
  Serial.begin(9600);
  while (!Serial) {}
}

void loop()
{
  if (Serial.find(query))
  {
    Serial.println(reply);
  }
  else
  {
    Serial.println("Listening...");
    Serial.flush();
  }
  
  delay(25);
}