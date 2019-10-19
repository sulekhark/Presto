#!/bin/bash

rm -f dbg_flows_run.out

z3 DbgFlows.datalog > dbg_flows.out 2>&1
python3 parse_z3_out.py DbgFlows.datalog dbg_flows.out >> dbg_flows_run.out 2>&1
