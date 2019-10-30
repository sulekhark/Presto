#!/bin/bash

rm -f full_run.out

z3 CIPtrAnalysis.datalog > cipa_analysis.out 2>&1
python3 parse_z3_out.py CIPtrAnalysis.datalog cipa_analysis.out >> full_run.out 2>&1

z3 ExcAnalysisIntraProc.datalog > exc_intra.out 2>&1
python3 parse_z3_out.py ExcAnalysisIntraProc.datalog exc_intra.out >> full_run.out 2>&1

z3 ExcAnalysisInterProc.datalog > exc_inter.out 2>&1
python3 parse_z3_out.py ExcAnalysisInterProc.datalog exc_inter.out >> full_run.out 2>&1

z3 ExcFlows.datalog > exc_flows.out 2>&1
python3 parse_z3_out.py ExcFlows.datalog exc_flows.out >> full_run.out 2>&1

z3 ExcAnalysisInterProc_inst.datalog > exc_inter_inst.out 2>&1
python3 parse_z3_out.py ExcAnalysisInterProc_inst.datalog exc_inter_inst.out >> full_run.out 2>&1
