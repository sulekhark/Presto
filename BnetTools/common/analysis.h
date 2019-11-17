
extern set<set<string>> getMethodFrontiers(set<string>& methTempl, set<vector<string>>& currentClauses);
extern set<string> getConsumedMethodFrontiers(set<string>& methTempl, set<vector<string>>& currentClauses);
extern set<string> getMethodObservables(set<string>& methTempl, set<vector<string>>& currentClauses);
extern set<string> getMethodClObservables(set<string>& methTempl, set<vector<string>>& currentClauses);
extern set<string> getMethodMemObservables(set<string>& methTempl, set<vector<string>>& currentClauses);
extern bool tupleMatchesObservable(set<string>& methTempl, string tup);
extern set<string> getObservableAntecedents(set<string>& methTempl, vector<string>& clause);
extern set<string> getNonObservableAntecedents(set<string>& methTempl, vector<string>& clause);
extern set<string> getSanityCheckTuples();
extern string getMainMethod();
extern set<string> getNonSummarizableMethods();
extern set<string> getClusterTupleTemplates();
extern string getCecId(string tup);
extern bool isCecTuple(string tup);
extern bool isEscapingRaceTuple(string tup);
