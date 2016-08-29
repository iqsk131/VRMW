using UnityEngine;
using System.Collections;

public interface ModelInterface {
	void attack (Transform target, int user, int attackTarget);
	void damaged (int user);
	void defend (int user);
	bool getDefendState ();
}
