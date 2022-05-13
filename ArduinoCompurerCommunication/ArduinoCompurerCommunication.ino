
#include <Servo.h>
String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
String commandString = "";
bool Iscrousover = true;
bool ToorCloss = true;

// defines pins numbers
const int trigPin = 10;
const int echoPin = 11;
// defines variables
long duration;
int distance;

int led1Pin = 2;
int led2Pin = 7;
int led3Pin = 4;

Servo motor;
int pos =0;



boolean isConnected = false;




void setup() {

   Serial.begin(9600);
  pinMode(led1Pin,OUTPUT);
  pinMode(led2Pin,OUTPUT);
  pinMode(led3Pin,OUTPUT);
  
digitalWrite(led3Pin, HIGH);

  
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input

  motor.attach(9);
  motor.write(0);
  
}

void loop() {
  
  // Clears the trigPin
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  // Sets the trigPin on HIGH state for 10 micro seconds
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  // Reads the echoPin, returns the sound wave travel time in microseconds
  if(ToorCloss){
    
     duration = pulseIn(echoPin, HIGH);
  }
 
  // Calculating the distance
  distance = duration * 0.034 / 2;
  // Prints the distance on the Serial Monitor
  //Serial.print("Distance: ");
  //Serial.println(distance);
 
  
    if(distance <10){
      ToorCloss=false;
      digitalWrite(led2Pin, HIGH);
      digitalWrite(led3Pin, LOW);      
      Serial.write(1);
      Serial.flush();
      delay(200);
      
      if(stringComplete)
      {
       stringComplete = false; 
       getCommand();      
       if(commandString.equals("Camera1"))
       {  
         turnLedOn(led2Pin);
         turnLedOn(led1Pin); 
         turnLedOff(led3Pin); 
         
         if(pos>=0)
         {
          for(pos=0; pos<=90; pos +=1)
          {
            motor.write(pos);
            digitalWrite(4,HIGH);
            digitalWrite(3,LOW);
            delay(30);
            }
          delay(3000);
          
          for(pos=90; pos>=0; pos -=1){
            motor.write(pos);
            digitalWrite(3,HIGH);
            digitalWrite(4,LOW);
            delay(30);  
            }
          digitalWrite(led1Pin, LOW);
          ToorCloss=true;
          }         
       }
        inputString = "";
        pos=0;
     }
   }else{
        digitalWrite(led3Pin, HIGH);
        digitalWrite(led2Pin, LOW);
      }
}


void getCommand()
{
  if(inputString.length()>0)
  {
     turnLedOn(led2Pin);
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
    }
  }
}
