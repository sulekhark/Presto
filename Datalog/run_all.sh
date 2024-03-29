#!/bin/bash

rm -f full_run.out

z3 ExcAnalysisIntraProc.datalog > exc_intra.out 2>&1
python3 parse_z3_out.py ExcAnalysisIntraProc.datalog exc_intra.out >> full_run.out 2>&1

z3 CIPtrAnalysis.datalog > cipa_analysis.out 2>&1
python3 parse_z3_out.py CIPtrAnalysis.datalog cipa_analysis.out >> full_run.out 2>&1

z3 ExcBugs.datalog > exc_bugs.out 2>&1
python3 parse_z3_out.py ExcBugs.datalog exc_bugs.out >> full_run.out 2>&1

z3 ExcChain.datalog > exc_chain.out 2>&1
python3 parse_z3_out.py ExcChain.datalog exc_chain.out >> full_run.out 2>&1

z3 ExcFlows.datalog > exc_flows.out 2>&1
python3 parse_z3_out.py ExcFlows.datalog exc_flows.out >> full_run.out 2>&1

z3 ExcAnalysis_inst.datalog > exc_analysis_inst.out 2>&1
python3 parse_z3_out.py ExcAnalysis_inst.datalog exc_analysis_inst.out >> full_run.out 2>&1

