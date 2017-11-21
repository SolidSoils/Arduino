/*
	The HC-06 is a slave-only Bluetooth module.
	It can only be programmed through its serial connection.
	This sketch shows how this can be done using SoftwareSerial.

	Connections:

	HC-06 pin        Arduino pin
	---------        -----------
	VCC              +3.3 Volt
	GND              GND
	TXD              Pin 10
	RXD              Pin 11

	NOTE: make sure to cut the voltage of RXD (5 V) to 3.3 V.

	The HC-06 can be configured using AT-commands. The commands need to be in upper case
	and must not be terminated with cr/lf. Commands are terminated using a 600 ms delay.

	The following AT-commands are supported:

	AT-command       Description
	----------       ----------------------------------
	AT+BAUD1         Sets serial comm. to 1,200 Baud
	AT+BAUD2         Sets serial comm. to 2,400 Baud
	AT+BAUD3         Sets serial comm. to 4,800 Baud
	AT+BAUD4         Sets serial comm. to 9,600 Baud
	AT+BAUD5         Sets serial comm. to 19,200 Baud
	AT+BAUD6         Sets serial comm. to 38,600 Baud
	AT+BAUD7         Sets serial comm. to 57,600 Baud
	AT+BAUD8         Sets serial comm. to 115,200 Baud
	AT+PN            Sets serial comm. protocol to Parity = None
	AT+PE            Sets serial comm. protocol to Parity = Even
	AT+PO            Sets serial comm. protocol to Parity = Odd
	AT+PIN1234       Sets connection password to 1234
	AT+NAMExxx       Sets Bluetooth device name to xxx
	AT+VERSION       Returns the device's version (e.g. "linvorV1")

	By default the HC-06 operates at 9,600 baud and no parity. The default PIN is 1234.

	Upload this sketch to your Arduino and use the Arduino IDE's serial monitor to issue AT-commands.
*/


#include <SoftwareSerial.h>

SoftwareSerial btSerial(10, 11); // RX, TX

void setup() {
  Serial.begin(57600);
  btSerial.begin(9600);
  delay(5000);
  // btSerial.print("AT+NAMESolidSoils");
  // delay(600);

  Serial.println("Uno initialized.");
}

void loop() {
  while (btSerial.available() > 0) {
    char c = btSerial.read();
    Serial.print(c);
  }
  
  while (Serial.available() > 0) {
    char c = Serial.read();
    btSerial.print(c);
    Serial.print(c);
  }

  delay(10);
}