// Sacred2Recomp - ReXGlue Recompiled Project

#include <rex/app/title_app.h>

#include "generated/default/Sacred2Recomp_init.h"

class Sacred2recompApp : public rex::app::TitleApp {
 public:
  Sacred2recompApp() : TitleApp("Sacred2Recomp", kPPCImageConfig) {}
};

REX_TITLE_APP(Sacred2recompApp)
