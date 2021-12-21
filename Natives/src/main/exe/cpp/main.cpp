#include "FRC_CAN_Reader_Native.h"

#include <iostream>
#include <string>

int main() {
  FRC_CAN_Reader_Native_Create(nullptr);

  std::string line;
  std::getline(std::cin, line);

  printf("Hello!\n");
}
