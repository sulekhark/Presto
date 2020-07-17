#!/bin/bash

find dynconfig/FaultInjectionSet -name torch-fault\* | xargs grep ExpressionFault | grep "\"\""
