#include <LiquidCrystal.h>

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
String commandString = "";

int led1Pin = 2;
int led2Pin = 7;
int led3Pin = 4;

boolean isConnected = false;




void setup() {
  
  Serial.begin(9600);
  pinMode(led1Pin,OUTPUT);
  pinMode(led2Pin,OUTPUT);
  pinMode(led3Pin,OUTPUT);
  
}

void loop() {

if(stringComplete)
{
  stringComplete = false;  
  getCommand();
  
  if(commandString.equals("STOP"))
  {
    turnLedOff(led1Pin);
    turnLedOff(led2Pin);
    turnLedOff(led3Pin);
     
  }
  
  else if(commandString.equals("Camera1"))
  {    
    
      turnLedOn(led1Pin);
   
  }
  else
    {
      turnLedOff(led1Pin);
    }   
  if(commandString.equals("Motor12"))
  {    
    
      turnLedOn(led2Pin);
      inputString = "";
    
  }
   else
    {
      turnLedOff(led2Pin);
    } 
     
  
  
  inputString = "";
}

}



boolean getLedState()
{
  boolean state = false;
  if(inputString.substring(8,10).equals("ON"))
  {
    state = true;
  }else
  {
    state = false;
  }
  return state;
}

void getCommand()
{
  if(inputString.length()>0)
  {
     commandString = inputString.substring(1,8);
  }
}

void turnLedOn(int pin)
{
  digitalWrite(pin,HIGH);
}

void turnLedOff(int pin)
{
  digitalWrite(pin,LOW);
}


String getTextToPrint()
{
  String value = inputString.substring(5,inputString.length()-2);
  return value;
}



void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
      Serial.println(inputString);
    }
  }
}
